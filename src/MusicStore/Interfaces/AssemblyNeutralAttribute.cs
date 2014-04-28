﻿using System;
using System.Runtime.Versioning;

namespace Microsoft.Net.Runtime
{
    [AssemblyNeutral]
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class AssemblyNeutralAttribute : Attribute
    {
    }
}