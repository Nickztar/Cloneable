using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Cloneable
{
    internal static class SymbolExtensions
    {
        public static bool TryGetAttribute(this ISymbol symbol, INamedTypeSymbol attributeType, out IEnumerable<AttributeData> attributes)
        {
            attributes = symbol.GetAttributes()
                .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
            return attributes.Any();
        }
        
        public static bool TryGetAttribute(this ISymbol symbol, string attributeDisplayStr, out AttributeData? attributes)
        {
            attributes = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeDisplayStr);
            return attributes is not null;
        }
        
        public static T? RetrieveArgument<T>(this AttributeData? attribute, string namedArgument)
        {
            return !attribute.TryGetArgument<T>(namedArgument, out var result) ? default : result;
        }
        
        public static bool TryGetArgument<T>(this AttributeData? attribute, string namedArgument, out T? value)
        {
            if (attribute?.NamedArguments.FirstOrDefault(x => x.Key == namedArgument).Value is not T typedArgument)
            {
                value = default;
                return false;
            }

            value = typedArgument;
            return true;
        }
        
        public static bool TryGetArgument<T>(this ISymbol symbol, string attributeDisplayStr, string namedArgument, out T? value)
        {
            var attribute = symbol
                .GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeDisplayStr);
            return TryGetArgument(attribute, namedArgument, out value);
        }
        
        public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
        {
            return symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
        }

        // ReSharper disable once InconsistentNaming
        public static string ToNullableFQF(this ISymbol symbol) =>
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
                )
            );

        // ReSharper disable once InconsistentNaming
        public static string ToFQF(this ISymbol symbol) =>
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        // ReSharper disable once InconsistentNaming
        public static string ToKnownInterfaceFQF(this ITypeSymbol symbol)
        {
            var iDictInterface = symbol.GetInterface("global::System.Collections.Generic.IDictionary<TKey, TValue>");
            var iListInterface = symbol.GetInterface("global::System.Collections.Generic.IList<T>");
            if (iDictInterface != null) return iDictInterface.ToFQF();
            else if (iListInterface != null) return iListInterface.ToFQF();
            return symbol.ToFQF();
        }

        public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
        {
            return symbol
                .GetAttributes()
                .FirstOrDefault(x => x.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) == true);
        }

        public static INamedTypeSymbol? GetInterface(this ITypeSymbol symbol, string interfaceFqn)
        {
            return symbol.AllInterfaces
                .FirstOrDefault(x => x.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == interfaceFqn);
        }

        public static ImmutableArray<ITypeSymbol>? GetIEnumerableTypeArguments(this ITypeSymbol symbol)
        {
            if (symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Collections.Generic.IEnumerable<T>") 
                return ((INamedTypeSymbol)symbol).TypeArguments;
            return symbol.GetInterface("global::System.Collections.Generic.IEnumerable<T>")?.TypeArguments;
        }

        public static ImmutableArray<ITypeSymbol>? GetIDictionaryTypeArguments(this ITypeSymbol symbol)
        {
            return symbol.GetInterface("global::System.Collections.Generic.IDictionary<TKey, TValue>")?.TypeArguments;
        }

        public static bool IsPossibleEnumerable(this ITypeSymbol symbol)
        {
            return !symbol.IsValueType && symbol.Name != "String" && (symbol.GetIEnumerableTypeArguments() != null || symbol.GetIDictionaryTypeArguments() != null);
        }
        
        
        public static StringBuilder AppendCode(this StringBuilder builder, string value, ushort indent = 0)
        {
            for (var i = 0; i < indent; i++)
            {
                builder.Append("    ");
            }

            return builder.AppendLine(value);
        }
        
        public static StringBuilder AppendCodeBlock(this StringBuilder builder, string value, ushort indent = 0)
        {
            if (string.IsNullOrEmpty(value))
                return builder;

            // Normalize all line endings to '\n' first so splitting is stable
            var normalized = value.Replace("\r\n", "\n").Replace("\r", "\n");

            foreach (var line in normalized.Split('\n'))
            {
                builder.AppendCode(line, indent);
            }

            return builder;
        }
        
        public static string ToCapitalized(this string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1, str.Length - 1);
        }
        
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> list, Func<T, TKey> propertySelector)
        {
            return list.GroupBy(propertySelector).Select(x => x.First());
        }
    }
}
