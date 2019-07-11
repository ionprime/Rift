﻿using System;

namespace Rift.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ResolveRoleAttribute : Attribute
    {
        public string Name { get; }

        public ResolveRoleAttribute(string name)
        {
            Name = name;
        }
    }
}
