using System;
using System.Collections.Generic;
using System.Linq;

namespace Cloneable.Sample;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        DoSimpleClone();
        DoSimpleExplicitClone();
        DoDeepClone();
        DoSafeDeepClone();
        DoListClone();
    }

    static void DoSimpleClone()
    {
        // Uses the Clone method on a class with no circular references
        var obj = new SimpleClone()
        {
            A = "salam",
            B = 100
        };
        var clone = obj.Clone();
        Console.WriteLine(clone);
        Console.WriteLine("Clone equals original: " + (clone == obj));
        Console.WriteLine();
    }

    static void DoSimpleExplicitClone()
    {
        // Uses the Clone method on a class with no circular references
        var obj = new SimpleCloneExplicit()
        {
            A = "salam",
            B = 100
        };
        var clone = obj.Clone();
        Console.WriteLine(clone);
        Console.WriteLine("Clone equals original: " + (clone == obj));
        Console.WriteLine();
    }

    static void DoDeepClone()
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
        Console.WriteLine(clone);
        Console.WriteLine("Clone equals original: " + (clone == deep));
        Console.WriteLine();
    }

    static void DoSafeDeepClone()
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
        Console.WriteLine(clone);
        Console.WriteLine("Clone equals original: " + (clone == parent));
        Console.WriteLine("Is parents child copied: " + (clone.Child != parent.Child));
        Console.WriteLine();
    }

    static void DoListClone()
    {
        // Uses the Clone method on a class with no circular references
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };
        var listClone = new ListClone()
        {
            A = "child",
            B = list.ToList()
        };
        var clone = listClone.CloneSafe();
        Console.WriteLine(clone);
        Console.WriteLine("Clone equals original: " + (clone == listClone));
        Console.WriteLine("List equals original: " + (clone.B == listClone.B));
        Console.WriteLine();
    }
}