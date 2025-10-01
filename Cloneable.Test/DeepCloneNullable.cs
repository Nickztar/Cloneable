#nullable enable
namespace Cloneable.Sample;

[Cloneable]
public partial class DeepCloneNullable
{
    public string A { get; set; }
    public SimpleClone? Simple { get; set; }

    public override string ToString()
    {
        return $"{nameof(DeepClone)}:{Environment.NewLine}" +
            $"\tA:\t{A}" +
            Environment.NewLine +
            $"\tSimple.A:\t{Simple?.A}" +
            Environment.NewLine +
            $"\tSimple.B:\t{Simple?.B}";
    }
}
    
[Cloneable]
public partial class DeepCloneNestedNullable
{
    public string A { get; set; }
    public SimpleClone?[] Simple { get; set; }
    public List<SimpleClone>? Simple2 { get; set; }
    public SimpleClone[]? Simple3 { get; set; }
    public List<SimpleClone?> Simple4 { get; set; }
}
