using Cloneable;

namespace Cloneable.Snapshots.Playground
{
    [Cloneable]
    public partial class DeepClone
    {
        public string A { get; set; }
        public SimpleClone Simple { get; set; }
    }
    
    
    [Cloneable]
    public partial class SimpleClone
    {
        public string A { get; set; }
        
        [IgnoreClone]
        public int B { get; set; }
    }
}
