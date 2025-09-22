#nullable enable
using System.Collections.Generic;
using Cloneable;
namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DeepListClone
    {
        public string A { get; set; }
        
        public List<SimpleClone> B { get; set; }
    }
    
    [Cloneable]
    public partial class SimpleClone
    {
        public string A { get; set; }
        
        [IgnoreClone]
        public int B { get; set; }
    }
}
