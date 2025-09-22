namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DictionaryClone
    {
        public string A { get; set; }
        
        public Dictionary<int, int> B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
