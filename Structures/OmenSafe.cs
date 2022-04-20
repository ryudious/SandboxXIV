using System.Threading.Tasks;

namespace SandboxXIV.Structures
{
    public static class OmenSafe
    {
        public static void DelayedRemove(Omen omen, float delay) => Task.Run((System.Action)(async () =>
       {
           await Task.Delay((int)((double)delay * 1000.0));
           omen.Remove();
       }));
    }
}
