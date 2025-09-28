using System;

namespace Cloneable.Sample;

[CloneableAttribute]
public partial class SimpleClone
{
    public string A { get; set; }
        
    [IgnoreClone]
    public int B { get; set; }

    public override string ToString()
    {
        return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
            $"\tA:\t{A}" +
            Environment.NewLine +
            $"\tB:\t{B}";
    }
}