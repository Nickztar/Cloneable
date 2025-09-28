using Cloneable.Sample;
using FluentAssertions;

namespace Cloneable.Test;

public class Tests
{
    [Fact]
    public void DoSimpleClone()
    {
        // Uses the Clone method on a class with no circular references
        var obj = new SimpleClone()
        {
            A = "salam",
            B = 100
        };
        var clone = obj.Clone();
        clone.Should().NotBe(obj);
    }

    [Fact]
    public void DoSimpleExplicitClone()
    {
        // Uses the Clone method on a class with no circular references
        var obj = new SimpleCloneExplicit()
        {
            A = "salam",
            B = 100
        };
        var clone = obj.Clone();
        clone.Should().NotBe(obj);
    }

    [Fact]
    public void DoDeepClone()
    {
        // Uses the Clone method on a class with no circular references
        var obj = new SimpleClone()
        {
            A = "salam",
            B = 100
        };
        var deep = new DeepClone()
        {
            A = "first",
            Simple = obj
        };
        var clone = deep.Clone();
        clone.Should().NotBe(deep);
    }

    [Fact]
    public void DoSafeDeepClone()
    {
        // Uses the Clone method on a class with no circular references
        var child = new SafeDeepCloneChild()
        {
            A = "child"
        };
        var parent = new SafeDeepClone()
        {
            A = "parent",
            Child = child
        };
        child.Parent = parent;
        var clone = parent.CloneSafe();
        clone.Should().NotBe(parent);
        clone.Child.Should().NotBe(parent.Child);
    }

    [Fact]
    public void DoSimpleListClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };
        var listClone = new ListClone()
        {
            A = "child",
            B = list
        };
        var clone = listClone.CloneSafe();
        clone.Should().NotBe(listClone);
        var listEqual = clone.B == listClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoNestedSimpleListClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };
        var listClone = new NestedListClone()
        {
            A = "child",
            B = new List<List<int>>() { list }
        };
        var clone = listClone.CloneSafe();
        clone.Should().NotBe(listClone);
        var listEqual = clone.B == listClone.B;
        listEqual.Should().BeFalse();
        var nestedListEqual = clone.B[0] == listClone.B[0];
        nestedListEqual.Should().BeFalse();
    }

    [Fact]
    public void DoNestedDeepListClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<SimpleClone>();
        for (int x = 0; x < 5; x++)
        {
            var obj = new SimpleClone()
            {
                A = "salam",
                B = Random.Shared.Next()
            };
            list.Add(obj);
        }
        var listClone = new NestedDeepListClone()
        {
            A = "child",
            B = new List<List<SimpleClone>>() { list }
        };
        var clone = listClone.CloneSafe();
        clone.Should().NotBe(listClone);
        var listEqual = clone.B == listClone.B;
        listEqual.Should().BeFalse();
        var nestedListEqual = clone.B[0] == listClone.B[0];
        nestedListEqual.Should().BeFalse();
    }

    [Fact]
    public void DoSimpleDictionaryClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };
        var listClone = new DictionaryClone()
        {
            A = "child",
            B = list.ToDictionary(x => x)
        };
        var clone = listClone.CloneSafe();
        clone.Should().NotBe(listClone);
        var listEqual = clone.B == listClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoDeepListClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<SimpleClone>();
        for (int x = 0; x < 5; x++)
        {
            var obj = new SimpleClone()
            {
                A = "salam",
                B = Random.Shared.Next()
            };
            list.Add(obj);
        }
        var listClone = new DeepListClone()
        {
            A = "child",
            B = list
        };
        var clone = listClone.CloneSafe();

        clone.B.Should().NotBeEquivalentTo(listClone.B);
        clone.Should().NotBe(listClone);
        var listEqual = clone.B == listClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoDeepDictionaryClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<SimpleClone>();
        for (int x = 0; x < 5; x++)
        {
            var obj = new SimpleClone()
            {
                A = "salam",
                B = Random.Shared.Next()
            };
            list.Add(obj);
        }
        var listClone = new DeepDictionaryClone()
        {
            A = "child",
            B = list.ToDictionary(x => x.B)
        };
        var clone = listClone.CloneSafe();

        clone.B.Should().NotBeEquivalentTo(listClone.B);
        clone.Should().NotBe(listClone);
        var listEqual = clone.B == listClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoSimpleEnumerableClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };
        var enumerableClone = new EnumerableClone()
        {
            A = "child",
            B = list.ToArray()
        };
        var clone = enumerableClone.CloneSafe();
        clone.Should().NotBe(enumerableClone);
        var listEqual = clone.B == enumerableClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoDeepEnumerableClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<SimpleClone>();
        for (int x = 0; x < 5; x++)
        {
            var obj = new SimpleClone()
            {
                A = "salam",
                B = Random.Shared.Next()
            };
            list.Add(obj);
        }
        var enumerableClone = new DeepEnumerableClone()
        {
            A = "child",
            B = list.ToArray()
        };
        var clone = enumerableClone.CloneSafe();
        clone.Should().NotBe(enumerableClone);
        var listEqual = clone.B == enumerableClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoSimpleArrayClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };
        var enumerableClone = new ArrayClone()
        {
            A = "child",
            B = list.ToArray()
        };
        var clone = enumerableClone.CloneSafe();
        clone.Should().NotBe(enumerableClone);
        var listEqual = clone.B == enumerableClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void DoDeepArrayClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<SimpleClone>();
        for (int x = 0; x < 5; x++)
        {
            var obj = new SimpleClone()
            {
                A = "salam",
                B = Random.Shared.Next()
            };
            list.Add(obj);
        }
        var enumerableClone = new DeepArrayClone()
        {
            A = "child",
            B = list.ToArray()
        };
        var clone = enumerableClone.CloneSafe();
        clone.Should().NotBe(enumerableClone);
        var listEqual = clone.B == enumerableClone.B;
        listEqual.Should().BeFalse();
    }

    [Fact]
    public void ThrowsOnNonNullable()
    {
        var deepClone = new DeepClone()
        {
            A = "test",
            Simple = null
        };

        Assert.Throws<NullReferenceException>(deepClone.Clone);
        Assert.Throws<NullReferenceException>(() => deepClone.CloneSafe());
    }

    [Fact]
    public void DoesntThrowOnNullable()
    {
        var deepClone = new DeepCloneNullable()
        {
            A = "test",
            Simple = null
        };

        deepClone.Clone();
        deepClone.CloneSafe();
    }


    [Fact]
    public void DoesntThrowOnNestedNullable()
    {
        var deepClone = new DeepCloneNestedNullable()
        {
            A = "test",
            Simple = [null],
            Simple3 = null,
            Simple2 = null,
            Simple4 = [null]
        };

        deepClone.Clone();
        deepClone.CloneSafe();
    }
}
