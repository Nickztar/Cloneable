namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DeepDictionaryClone
    {
        public string A { get; set; }
        
        public Dictionary<int, SimpleClone> B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
