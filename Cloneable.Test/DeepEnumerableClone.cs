namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DeepEnumerableClone
    {
        public string A { get; set; }
        
        public IEnumerable<SimpleClone> B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
