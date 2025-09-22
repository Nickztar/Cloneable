namespace Cloneable.Sample
{
    [Cloneable]
    public partial class NestedListClone
    {
        public string A { get; set; }
        
        public List<List<int>> B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
