using System;
using System.Collections.Generic;
using System.Text;

namespace RoyalMessenger.Contracts
{
    /// <summary>Indicates that the target of this annotation can be <see langword="null" />.</summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class NullableAttribute : Attribute { }
}
