// Decompiled with JetBrains decompiler
// Type: SandboxXIV.Structures.OmenSafe
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using System.Threading.Tasks;

namespace SandboxXIV.Structures
{
  public static class OmenSafe
  {
    public static void DelayedRemove(Omen omen, float delay) => Task.Run((System.Action) (async () =>
    {
      await Task.Delay((int) ((double) delay * 1000.0));
      omen.Remove();
    }));
  }
}
