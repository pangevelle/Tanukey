using System;

namespace Tanukey
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]

    public class ObfuscatedAttribute : Attribute
    {
    }
}
