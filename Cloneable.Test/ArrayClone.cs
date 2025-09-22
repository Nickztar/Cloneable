namespace Cloneable.Sample
{
    [Cloneable]
    public partial class ArrayClone
    {
        public string A { get; set; }
        
        public int[] B { get; set; }

        public override string ToString()
        {
            return $"{nameof(SimpleClone)}:{Environment.NewLine}" +
                $"\tA:\t{A}" +
                Environment.NewLine +
                $"\tB:\t{B}";
        }
    }
}
