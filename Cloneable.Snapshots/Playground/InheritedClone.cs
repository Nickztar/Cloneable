using Cloneable;

namespace Cloneable.Snapshots.Playground
{
    [Cloneable]
    public partial class InheritedDeepClone : BaseClonable
    {
        public string A { get; set; }
        public InheritedSimpleClone Simple { get; set; }
    }
    
    [Cloneable]
    public partial class InheritedSimpleClone : BaseClone
    {
        public string A { get; set; }
        
        [IgnoreClone]
        public int B { get; set; }
    }
    
    public class BaseClone
    {
        public int Id { get; set; }
    }
    
    [Cloneable]
    public partial class BaseClonable
    {
        public int Other { get; set; }
    }

}
