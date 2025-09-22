#nullable enable

namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DeepListClone
    {
        public string A { get; set; }
        
        public List<SimpleClone> B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
