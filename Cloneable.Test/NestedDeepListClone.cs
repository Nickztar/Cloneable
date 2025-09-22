namespace Cloneable.Sample
{
    [Cloneable]
    public partial class NestedDeepListClone
    {
        public string A { get; set; }
        
        public List<List<SimpleClone>> B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
