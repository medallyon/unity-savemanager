using System;

namespace Medallyon
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SaveAttribute : Attribute
    {
    }
}
