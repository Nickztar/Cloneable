using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Cloneable.Sample;

[Cloneable]
public partial class ListClone
{
    public string A { get; set; }

    public List<int> B { get; set; }
    public List<ListClone> Self { get; set; }
    public List<ListClone?> NullableList { get; set; }
    public Dictionary<int, ListClone> Dict { get; set; }
    public Queue<int> Queue { get; set; }
    public ConcurrentQueue<int> ConcurrentQueue { get; set; }
    public int[] Arr { get; set; }
    public ArrayList ArrList { get; set; }
    public Stack<int> Stack { get; set; }
    public ConcurrentStack<int> ConcurrentStack { get; set; }
    public LinkedList<int> LinkedList { get; set; }
    public ConcurrentDictionary<int, int> ConcurrentDictionary { get; set; }
    public Queue<SimpleClone> Queue2 { get; set; }
    public ConcurrentQueue<SimpleClone> ConcurrentQueue2 { get; set; }
    public SimpleClone[] Arr2 { get; set; }
    public Stack<SimpleClone> Stack2 { get; set; }
    public ConcurrentStack<SimpleClone> ConcurrentStack2 { get; set; }
    public LinkedList<SimpleClone> LinkedList2 { get; set; }
    public ConcurrentDictionary<int, SimpleClone> ConcurrentDictionary2 { get; set; }
    public SortedList<int, int> SortList { get; set; }
    public SortedList<int, SimpleClone> SortedList2 { get; set; }



    public override string ToString()
    {
        return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
            $"\tA:\t{A}" +
            Environment.NewLine +
            $"\tB:\t{B}";
    }
}