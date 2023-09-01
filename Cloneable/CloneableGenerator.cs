using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cloneable;

[Generator]
public class CloneableGenerator : ISourceGenerator
{
    private const string PreventDeepCopyKeyString = "PreventDeepCopy";
    private const string ExplicitDeclarationKeyString = "ExplicitDeclaration";

    private const string CloneableNamespace = "Cloneable";
    private const string CloneableAttributeString = "CloneableAttribute";
    private const string CloneAttributeString = "CloneAttribute";
    private const string IgnoreCloneAttributeString = "IgnoreCloneAttribute";

    private const string cloneableAttributeText = $$"""
using System;

namespace {{CloneableNamespace}}
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class {{CloneableAttributeString}} : Attribute
    {
        public {{CloneableAttributeString}}()
        {
        }

        public bool {{ExplicitDeclarationKeyString}} { get; set; }
    }
}

""";

    private const string clonePropertyAttributeText = $$"""
using System;

namespace {{CloneableNamespace}}
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class {{CloneAttributeString}} : Attribute
    {
        public {{CloneAttributeString}}()
        {
        }

        public bool {{PreventDeepCopyKeyString}} { get; set; }
    }
}

""";

    private const string ignoreClonePropertyAttributeText = $$"""
using System;

namespace {{CloneableNamespace}}
{
  [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
  public sealed class {{IgnoreCloneAttributeString}} : Attribute
  {
      public {{IgnoreCloneAttributeString}}()
      {
      }
  }
}

""";

    //TODO: Add CustomCloneAttribute?
    private INamedTypeSymbol? cloneableAttribute;
    private INamedTypeSymbol? ignoreCloneAttribute;
    private INamedTypeSymbol? cloneAttribute;

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        InjectCloneableAttributes(context);
        GenerateCloneMethods(context);
    }

    private void GenerateCloneMethods(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        Compilation compilation = GetCompilation(context);

        InitAttributes(compilation);

        var classSymbols = GetClassSymbols(compilation, receiver).ToList();
        foreach (var classSymbol in classSymbols)
        {
            if (!classSymbol.TryGetAttribute(cloneableAttribute!, out var attributes))
                continue;

            var attribute = attributes.Single();
            var isExplicit = (bool?)attribute.NamedArguments.FirstOrDefault(e => e.Key.Equals(ExplicitDeclarationKeyString)).Value.Value ?? false;
            var hasDuplicateName = classSymbols.Any(x => !SymbolEqualityComparer.Default.Equals(x, classSymbol) && x.Name == classSymbol.Name); //Fix issue where two classes have the same name
            context.AddSource($"{(hasDuplicateName ? $"{classSymbol.ContainingNamespace}." : null)}{classSymbol.Name}_cloneable.cs", SourceText.From(CreateCloneableCode(classSymbol, isExplicit), Encoding.UTF8));
        }
    }

    private void InitAttributes(Compilation compilation)
    {
        cloneableAttribute = compilation.GetTypeByMetadataName($"{CloneableNamespace}.{CloneableAttributeString}")!;
        cloneAttribute = compilation.GetTypeByMetadataName($"{CloneableNamespace}.{CloneAttributeString}")!;
        ignoreCloneAttribute = compilation.GetTypeByMetadataName($"{CloneableNamespace}.{IgnoreCloneAttributeString}")!;
    }

    private static Compilation GetCompilation(GeneratorExecutionContext context)
    {
        var options = context.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;

        var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(cloneableAttributeText, Encoding.UTF8), options)).
            AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(clonePropertyAttributeText, Encoding.UTF8), options)).
            AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(ignoreClonePropertyAttributeText, Encoding.UTF8), options));
        return compilation;
    }

    private string CreateCloneableCode(INamedTypeSymbol classSymbol, bool isExplicit)
    {
        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var fieldAssignmentsCode = GenerateFieldAssignmentsCode(classSymbol, isExplicit);
        var fieldAssignmentsCodeSafe = fieldAssignmentsCode.Select(x =>
        {
            if (x.isEnumerable)
                return x.line.Replace("#CLONE#", "CloneSafe(referenceChain)");
            if (x.isCloneable)
                return $"{x.line}Safe(referenceChain)";
            return x.line;
        });
        var fieldAssignmentsCodeFast = fieldAssignmentsCode.Select(x =>
        {
            if (x.isEnumerable)
                return x.line.Replace("#CLONE#", "Clone()");
            if (x.isCloneable)
                return $"{x.line}()";
            return x.line;
        });

        return $$"""
 using System.Collections.Generic;
 using System.Linq;
 namespace {{namespaceName}}
 {
     {{GetAccessModifier(classSymbol)}} partial class {{classSymbol.Name}}
     {
         /// <summary>
         /// Creates a copy of {{classSymbol.Name}} with NO circular reference checking. This method should be used if performance matters.
         ///
         /// <exception cref="StackOverflowException">Will occur on any object that has circular references in the hierarchy.</exception>
         /// </summary>
         public {{classSymbol.ToFQF()}} Clone()
         {
             return new {{classSymbol.ToFQF()}}
             {
 {{string.Join($",{Environment.NewLine}", fieldAssignmentsCodeFast)}}
             };
         }
 
         /// <summary>
         /// Creates a copy of {{classSymbol.Name}} with circular reference checking. If a circular reference was detected, only a reference of the leaf object is passed instead of cloning it.
         /// </summary>
         /// <param name="referenceChain">Should only be provided if specific objects should not be cloned but passed by reference instead.</param>
         public {{classSymbol.ToFQF()}} CloneSafe(Stack<object> referenceChain = null)
         {
             if(referenceChain?.Contains(this) == true)
                 return this;
             referenceChain ??= new Stack<object>();
             referenceChain.Push(this);
             var result = new {{classSymbol.ToFQF()}}
             {
 {{string.Join($",{Environment.NewLine}", fieldAssignmentsCodeSafe)}}
             };
             referenceChain.Pop();
             return result;
         }
     }
 }
 """;
    }

    private IEnumerable<(string line, bool isCloneable, bool isEnumerable)> GenerateFieldAssignmentsCode(INamedTypeSymbol classSymbol, bool isExplicit )
    {
        var fieldNames = GetCloneableProperties(classSymbol, isExplicit);

        var fieldAssignments = fieldNames.Select(field => IsFieldCloneable(field, classSymbol)).
            OrderBy(x => x.isCloneable).
            Select(x => (GenerateAssignmentCode(x.item, x.isCloneable, x.isEnumerable), x.isCloneable, x.isEnumerable));
        return fieldAssignments;
    }

    private string GenerateAssignmentCode(IPropertySymbol symbol, bool isCloneable, bool isEnumerable)
    {
        var name = symbol.Name;
        if (isEnumerable)
        {
            return $@"                {name} = {GenerateEnumerableConversionCode($"this.{name}", symbol.Type)}";
        }
        if (isCloneable)
        {
            return $@"                {name} = this.{name}{(symbol.NullableAnnotation == NullableAnnotation.Annotated ? "?" : "")}.Clone";
        }

        return $@"                {name} = this.{name}";
    }

    private string GenerateEnumerableConversionCode(string name, ITypeSymbol type, int depth = 1)
    {
        var arguments = type.GetIDictionaryTypeArguments() ?? type.GetIEnumerableTypeArguments();
        if (arguments == null) return name;
        var argumentName = new string('x', depth);
        if (type is IArrayTypeSymbol arraySymbol)
        {
            return $"{name}.Select({argumentName} => {GenerateEnumerableTypeCloneCode(argumentName, arguments.Value[0], depth)}).ToArray()";
        }
        var typeAsEnumerable = $"global::System.Collections.Generic.IEnumerable<{arguments.Value.ElementAtOrDefault(0)?.ToFQF()}>";
        var argumentsAsKeyValuePair = $"global::System.Collections.Generic.KeyValuePair<{arguments.Value.ElementAtOrDefault(0)?.ToFQF()}, {arguments.Value.ElementAtOrDefault(1)?.ToFQF()}>";
        var typeAsKeyValuePair = $"global::System.Collections.Generic.IEnumerable<{argumentsAsKeyValuePair}>";
        var isConstructableWithSelf = ((INamedTypeSymbol)type).Constructors.Any(constructor =>
            constructor.Parameters.Any(param => param.Type.ToFQF() == type.ToKnownInterfaceFQF())
        );
        var isConstructableWithEnumerable = ((INamedTypeSymbol)type).Constructors.Any(constructor =>
            constructor.Parameters.Any(param => param.Type.ToFQF() == typeAsEnumerable)
        );
        var isConstructableWithKeyValuePair = ((INamedTypeSymbol)type).Constructors.Any(constructor =>
            constructor.Parameters.Any(param => param.Type.ToFQF() == typeAsKeyValuePair)
        );
        if (arguments.Value.Any(x => !x.IsValueType))
        {
            //Note: Should support "most" of the commonly used collections https://learn.microsoft.com/en-us/dotnet/standard/collections/commonly-used-collection-types
            //Does not really support: Hashtable (Depricated), ArrayList (Depricated), SortedList (Does currently only support value types.
            if (isConstructableWithEnumerable)
            {
                return $"new {type.ToNullableFQF()}({name}.Select({argumentName} => {GenerateEnumerableTypeCloneCode(argumentName, arguments.Value[0], depth)}))";
            }
            else if (isConstructableWithKeyValuePair)
            {
                return $"new {type.ToNullableFQF()}({name}.Select({argumentName} => new {argumentsAsKeyValuePair}({GenerateEnumerableTypeCloneCode($"{argumentName}.Key", arguments.Value[0], depth)}, {GenerateEnumerableTypeCloneCode($"{argumentName}.Value", arguments.Value[1], depth)})))";
            }
            else if (type.ToFQF() == typeAsEnumerable)
            {
                return $"{name}.Select({argumentName} => {GenerateEnumerableTypeCloneCode(argumentName, arguments.Value[0], depth)})";
            }
            return name;
        }
        if (isConstructableWithSelf)
        {
            return $"new {type.ToNullableFQF()}({name})";
        }
        else if (isConstructableWithEnumerable)
        {
            return $"new {type.ToNullableFQF()}({name}.Select({argumentName} => {argumentName}))";
        }
        else if (isConstructableWithKeyValuePair)
        {
            return $"new {type.ToNullableFQF()}({name}.Select({argumentName} => new {argumentsAsKeyValuePair}({argumentName}.Key, {argumentName}.Value)))";
        }
        else if (type.ToFQF() == typeAsEnumerable)
        {
            return $"{name}.Select({argumentName} => {argumentName})";
        }
        return name;
    }

    private string GenerateEnumerableTypeCloneCode(string name, ITypeSymbol type, int depth)
    {
        if (type.IsPossibleEnumerable()) //If it is a nested enumerable, repeat the process
        {
            return GenerateEnumerableConversionCode(name, type, depth + 1);
        }
        if (!type.TryGetAttribute(cloneableAttribute!, out var attributes))
        {
            return name;
        }
        var preventDeepCopy = (bool?)attributes.Single().NamedArguments.FirstOrDefault(e => e.Key.Equals(PreventDeepCopyKeyString)).Value.Value ?? false;
        if (preventDeepCopy) return name;
        return $"{name}.#CLONE#";
    }

    private (IPropertySymbol item, bool isCloneable, bool isEnumerable) IsFieldCloneable(IPropertySymbol x, INamedTypeSymbol classSymbol)
    {
        if (SymbolEqualityComparer.Default.Equals(x.Type, classSymbol))
        {
            return (x, false, false);
        }

        if (x.Type.IsPossibleEnumerable())
        {
            return (x, false, true);
        }

        if (!x.Type.TryGetAttribute(cloneableAttribute!, out var attributes))
        {
            return (x, false, false);
        }

        var preventDeepCopy = (bool?)attributes.Single().NamedArguments.FirstOrDefault(e => e.Key.Equals(PreventDeepCopyKeyString)).Value.Value ?? false;
        return (item: x, !preventDeepCopy, false);
    }

    private static string GetAccessModifier(INamedTypeSymbol classSymbol)
    {
        return classSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
    }

    private IEnumerable<IPropertySymbol> GetCloneableProperties(ITypeSymbol classSymbol, bool isExplicit)
    {
        var targetSymbolMembers = classSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(x => x.SetMethod is not null &&
                x.CanBeReferencedByName);
        if (isExplicit)
        {
            return targetSymbolMembers.Where(x => x.HasAttribute(cloneAttribute!));
        }
        else
        {
            return targetSymbolMembers.Where(x => !x.HasAttribute(ignoreCloneAttribute!));
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetClassSymbols(Compilation compilation, SyntaxReceiver receiver)
    {
        return receiver.CandidateClasses.Select(@class => GetClassSymbol(compilation, @class));
    }

    private static INamedTypeSymbol GetClassSymbol(Compilation compilation, ClassDeclarationSyntax @class)
    {
        var model = compilation.GetSemanticModel(@class.SyntaxTree);
        var classSymbol = model.GetDeclaredSymbol(@class)!;
        return classSymbol;
    }

    private static void InjectCloneableAttributes(GeneratorExecutionContext context)
    {
        context.AddSource(CloneableAttributeString, SourceText.From(cloneableAttributeText, Encoding.UTF8));
        context.AddSource(CloneAttributeString, SourceText.From(clonePropertyAttributeText, Encoding.UTF8));
        context.AddSource(IgnoreCloneAttributeString, SourceText.From(ignoreClonePropertyAttributeText, Encoding.UTF8));
    }
}
