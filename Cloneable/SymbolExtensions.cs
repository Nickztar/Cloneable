using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cloneable;

internal static class SymbolExtensions
{
    public static bool TryGetAttribute(this ISymbol symbol, INamedTypeSymbol attributeType, out IEnumerable<AttributeData> attributes)
    {
        attributes = symbol.GetAttributes()
            .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
        return attributes.Any();
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
}