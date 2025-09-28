using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloneable;

[Generator]
public class IncrementalCloneableGenerator : IIncrementalGenerator
{
    private const string ClonableAttribute = "Cloneable.CloneableAttribute";
    private const string CloneAttribute = "Cloneable.CloneAttribute";
    private const string IgnoreCloneAttribute = "Cloneable.IgnoreCloneAttribute";
    private const string PreventDeepCopyKeyString = "PreventDeepCopy";
    private const string ExplicitDeclarationKeyString = "ExplicitDeclaration";
        
    private const string ClonableAttributeText = $$"""
                                                   #nullable enable
                                                   using System;
                                                   namespace Cloneable
                                                   {
                                                       [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
                                                       sealed class CloneableAttribute : Attribute
                                                       {
                                                           public CloneableAttribute()
                                                           {
                                                           }
                                                           
                                                           public bool {{ExplicitDeclarationKeyString}} { get; set; }
                                                       }
                                                   }
                                                   """;
        
    private const string CloneAttributeText = $$"""
                                                #nullable enable
                                                using System;
                                                namespace Cloneable
                                                {
                                                    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
                                                    sealed class CloneAttribute : Attribute
                                                    {
                                                        public CloneAttribute()
                                                        {
                                                        }
                                                        
                                                        public bool {{PreventDeepCopyKeyString}} { get; set; }
                                                    }
                                                }
                                                """;
        
        
    private const string IgnoreCloneAttributeText = $$"""
                                                      #nullable enable
                                                      using System;
                                                      namespace Cloneable
                                                      {
                                                          [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
                                                          sealed class IgnoreCloneAttribute : Attribute
                                                          {
                                                              public IgnoreCloneAttribute()
                                                              {
                                                              }
                                                          }
                                                      }
                                                      """;
    
    internal record struct ClonableMember(
        string PropertyName,
        bool IsDeepClonable,
        bool Nullable,
        string? AccessorStr
    );
    internal record struct ClonableItem(
        string Name, 
        string FQF, 
        string Namespace, 
        string AccessModifier, 
        ClonableMember[] Members
    );
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("Cloneable.CloneableAttribute.g.cs", ClonableAttributeText);
            ctx.AddSource("Cloneable.CloneAttribute.g.cs", CloneAttributeText);
            ctx.AddSource("Cloneable.IgnoreCloneAttribute.g.cs", IgnoreCloneAttributeText);
        });

        // Create a provider to gather all properties with the BerlexDescription attribute
        var targetProperties = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ClonableAttribute,
                predicate: (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => GetPropertySymbol(ctx));
        context.RegisterSourceOutput(targetProperties, (ctx, classSymbol) =>
        {
            if (classSymbol is null) return;
            var classSource = ProcessClass(classSymbol.Value);
            ctx.AddSource($"{classSymbol.Value.Namespace}.{classSymbol.Value.Name}.cloneable.g.cs", SourceText.From(classSource, Encoding.UTF8));
        });
    }

    private static ClonableItem? GetPropertySymbol(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol namedSymbol)
            return null;
        var requireExplicit = context.Attributes.FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == ClonableAttribute)?
            .NamedArguments.FirstOrDefault(x => x.Key == ExplicitDeclarationKeyString).Value.Value as bool? is true;
        return new ClonableItem(
            namedSymbol.Name, 
            namedSymbol.ToFQF(), 
            namedSymbol.ContainingNamespace.ToDisplayString(), 
            namedSymbol.DeclaredAccessibility switch
            {
                Accessibility.Internal => "internal",
                _ => "public",
            }, 
            GetMembers(namedSymbol, requireExplicit).ToArray()
        );
    }
    
    private static IEnumerable<ClonableMember> GetMembers(INamedTypeSymbol symbol, bool requireExplicit)
    {
        foreach (var member in symbol.GetMembers())
        {
            if (
                member is not IPropertySymbol { IsReadOnly: false }
            ) 
                continue;
                
            var attributes = member.GetAttributes();
            var cloneAttr = attributes.FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == CloneAttribute);
            if (attributes.Any(x => x.AttributeClass?.ToDisplayString() == IgnoreCloneAttribute))
                continue;
            if (requireExplicit && cloneAttr is null)
                continue;
            var type = member switch
            {
                IPropertySymbol prop => prop.Type,
                IFieldSymbol field => field.Type,
                _ => null
            };
            var isEnumerable = type?.IsPossibleEnumerable() ?? false;
            var isDeepClonable = type is not null && type.TryGetAttribute(ClonableAttribute, out var attribute) && attribute.RetrieveArgument<bool?>(PreventDeepCopyKeyString) is null or true;
            var clonableMember = new ClonableMember(
                member.Name, 
                isDeepClonable,
                type?.NullableAnnotation is NullableAnnotation.Annotated,
                null
            );
            if (isEnumerable && type is not null)
            {
                yield return clonableMember with { AccessorStr = BuildEnumerableAccessor(clonableMember.PropertyName, type) };
            }
            else
            {
                yield return clonableMember;
            }
        }
    }
    
    private static string? BuildEnumerableAccessor(
        string previousAccessor,
        ITypeSymbol type
    )
    {
        var newAccessor = previousAccessor + "x";
        var isNullable = type.NullableAnnotation is NullableAnnotation.Annotated;
        var nullablePrevious = isNullable ? $"{previousAccessor}?" : previousAccessor;
        if (!type.IsPossibleEnumerable())
        {
            // Check if this type is supposed to be "Cloneable"
            var isDeepClonable = type.TryGetAttribute(ClonableAttribute, out var attribute) && attribute.RetrieveArgument<bool?>(PreventDeepCopyKeyString) is null or true;
            return isDeepClonable ? $"{nullablePrevious}.#CLONE#" : previousAccessor;
        }
        var arguments = type.GetIDictionaryTypeArguments() ?? type.GetIEnumerableTypeArguments() ?? [];
        if (!arguments.Any()) return previousAccessor; // Not much to do
        if (type is IArrayTypeSymbol)
        {
            return $"{nullablePrevious}.Select({newAccessor} => {BuildEnumerableAccessor(newAccessor, arguments[0])}).ToArray()";
        }
        var typeAsEnumerable = $"global::System.Collections.Generic.IEnumerable<{arguments.ElementAtOrDefault(0)?.ToFQF()}>";
        var argumentsAsKeyValuePair = $"global::System.Collections.Generic.KeyValuePair<{arguments.ElementAtOrDefault(0)?.ToFQF()}, {arguments.ElementAtOrDefault(1)?.ToFQF()}>";
        var typeAsKeyValuePair = $"global::System.Collections.Generic.IEnumerable<{argumentsAsKeyValuePair}>";
        var isConstructableWithSelf = ((INamedTypeSymbol)type).Constructors.Any(constructors =>
            constructors.Parameters.Any(param => param.Type.ToFQF() == type.ToKnownInterfaceFQF())
        );
        var isConstructableWithEnumerable = ((INamedTypeSymbol)type).Constructors.Any(constructors => 
            constructors.Parameters.Any(param => param.Type.ToFQF() == typeAsEnumerable)
        );
        var isConstructableWithKeyValuePair = ((INamedTypeSymbol)type).Constructors.Any(constructors =>
            constructors.Parameters.Any(param => param.Type.ToFQF() == typeAsKeyValuePair)
        );
        var nullableCheck = isNullable ? $"{previousAccessor} is null ? null : " : "";
        var nullableFQF = type.ToNullableFQF().TrimEnd('?'); // Since we null check, we are fine not actually caring
        if (arguments.Any(x => !x.IsValueType))
        {
            //Note: Should support "most" of the commonly used collections https://learn.microsoft.com/en-us/dotnet/standard/collections/commonly-used-collection-types
            //Does not really support: Hashtable (Depricated), ArrayList (Depricated), SortedList (Does currently only support value types. 
            if (isConstructableWithEnumerable)
            {
                return nullableCheck + $"new {nullableFQF}({previousAccessor}.Select({newAccessor} => {BuildEnumerableAccessor(newAccessor, arguments[0])}))";
            }

            if (isConstructableWithKeyValuePair)
            {
                return nullableCheck + $"new {nullableFQF}({previousAccessor}.Select({newAccessor} => new {argumentsAsKeyValuePair}({BuildEnumerableAccessor($"{newAccessor}.Key", arguments[0])}, {BuildEnumerableAccessor($"{newAccessor}.Value", arguments[1])})))";
            }

            if (type.ToFQF() == typeAsEnumerable)
            {
                return nullableCheck + $"{previousAccessor}.Select({newAccessor} => {BuildEnumerableAccessor(newAccessor, arguments[0])})";
            }
            return previousAccessor;
        }
        if (isConstructableWithSelf)
        {
            return nullableCheck + $"new {nullableFQF}({previousAccessor})";
        }
        if (isConstructableWithEnumerable)
        {
            return nullableCheck + $"new {nullableFQF}({previousAccessor}.Select({newAccessor} => {newAccessor}))";
        }
        if (isConstructableWithKeyValuePair)
        {
            return nullableCheck + $"new {nullableFQF}({previousAccessor}.Select({newAccessor} => new {argumentsAsKeyValuePair}({newAccessor}.Key, {newAccessor}.Value)))";
        }
        if (type.ToFQF() == typeAsEnumerable)
        {
            return nullableCheck + $"{previousAccessor}.Select({newAccessor} => {newAccessor})";
        }
        return previousAccessor;
    }
    
    private string ProcessClass(ClonableItem clonable)
    {
        // Begin building the generated source
        var source = new StringBuilder($$"""
                                         // <auto-generated/>
                                         #nullable enable
                                         using System;
                                         using System.Linq;
                                         
                                         namespace {{clonable.Namespace}};
                                         {{clonable.AccessModifier}} partial class {{clonable.Name}}
                                         {

                                         """);

        // Build fast "unsafe" clone
        source.AppendCodeBlock($"""
        /// <summary>
        /// Creates a copy of {clonable.Name} with NO circular reference checking. This method should be used if performance matters.
        /// <exception cref="StackOverflowException">Will occur on any object that has circular references in the hierarchy.</exception>
        /// </summary>
        """, 1);
        source.AppendCode($"public {clonable.FQF} Clone()", 1);
        source.AppendCode("{", 1);
        source.AppendCode($"return new {clonable.FQF}", 2);
        source.AppendCode("{", 2);
        foreach (var member in clonable.Members)
        {
            if (!string.IsNullOrEmpty(member.AccessorStr))
            {
                source.AppendCode($"{member.PropertyName} = {member.AccessorStr!.Replace("#CLONE#", "Clone()")},", 3);
                continue;
            }
            if (member.IsDeepClonable)
            {
                source.AppendCode($"{member.PropertyName} = this.{member.PropertyName}{(member.Nullable ? "?" : "")}.Clone(),", 3);
                continue;
            }
            source.AppendCode($"{member.PropertyName} = this.{member.PropertyName},", 3);
        }
        source.AppendCode("};", 2);
        source.AppendCode("}", 1);
        
        // Build fast "safe" code
        source.AppendCodeBlock($"""
        /// <summary>
        /// Creates a copy of {clonable.Name} with circular reference checking. If a circular reference was detected, only a reference of the leaf object is passed instead of cloning it.
        /// </summary>
        /// <param name="referenceChain">Should only be provided if specific objects should not be cloned but passed by reference instead.</param>
        """, 1);
        source.AppendCode($"public {clonable.FQF} CloneSafe(global::System.Collections.Generic.Stack<object>? referenceChain = null)", 1);
        source.AppendCode("{", 1);
        source.AppendCode("if (referenceChain?.Contains(this) == true) return this;", 2);
        source.AppendCode("referenceChain ??= new global::System.Collections.Generic.Stack<object>();", 2);
        source.AppendCode("referenceChain.Push(this);", 2);
        source.AppendCode($"var clone = new {clonable.FQF}", 2);
        source.AppendCode("{", 2);
        foreach (var member in clonable.Members)
        {
            if (!string.IsNullOrEmpty(member.AccessorStr))
            {
                source.AppendCode($"{member.PropertyName} = {member.AccessorStr!.Replace("#CLONE#", "CloneSafe(referenceChain)")},", 3);
                continue;
            }
            if (member.IsDeepClonable)
            {
                source.AppendCode($"{member.PropertyName} = this.{member.PropertyName}{(member.Nullable ? "?" : "")}.CloneSafe(referenceChain),", 3);
                continue;
            }
            source.AppendCode($"{member.PropertyName} = this.{member.PropertyName},", 3);
        }
        source.AppendCode("};", 2);
        source.AppendCode("referenceChain.Pop();", 2);
        source.AppendCode("return clone;", 2);
        source.AppendCode("}", 1);

        source.AppendCode("}");

        return source.ToString();
    }
}