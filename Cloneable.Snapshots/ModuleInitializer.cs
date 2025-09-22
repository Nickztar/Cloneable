using System.Runtime.CompilerServices;

namespace Cloneable.Snapshots;

public static class ModuleInitializers
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
