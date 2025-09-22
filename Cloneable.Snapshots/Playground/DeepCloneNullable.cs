#nullable enable
using Cloneable;
using System.Collections.Generic;
namespace Cloneable.Sample
{
    [Cloneable]
    public partial class DeepCloneNullable
    {
        public string A { get; set; }
        public SimpleClone? Simple { get; set; }
    }
    
    [Cloneable]
    public partial class DeepCloneNestedNullable
    {
        public string A { get; set; }
        public SimpleClone?[] Simple { get; set; }
    }
    
    
    [Cloneable]
    public partial class SimpleClone
    {
        public string A { get; set; }
        
        [IgnoreClone]
        public int B { get; set; }
    }
}
