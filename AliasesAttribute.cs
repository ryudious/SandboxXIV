﻿using System;

namespace SandboxXIV
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AliasesAttribute : Attribute
    {
        public string[] Aliases { get; }

        public AliasesAttribute(params string[] aliases) => this.Aliases = aliases;
    }
}
