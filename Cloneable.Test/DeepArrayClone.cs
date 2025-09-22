namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DeepArrayClone
    {
        public string A { get; set; }
        
        public SimpleClone[] B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
