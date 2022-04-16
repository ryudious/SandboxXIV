// Decompiled with JetBrains decompiler
// Type: SandboxXIV.CommandAttribute
// Assembly: SandboxXIV, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9B1263EC-C2A7-47EB-B77D-B4BE83A58FC3
// Assembly location: C:\Users\axelm\Desktop\VoidXIV.dll

using System;

namespace SandboxXIV
{
  [AttributeUsage(AttributeTargets.Method)]
  public class CommandAttribute : Attribute
  {
    public string Command { get; }

    public CommandAttribute(string command) => this.Command = command;
  }
}
