using System;

namespace MaSch.CommandLineTools.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReturnableExitCodeAttribute : Attribute
    {
        public string Description { get; }

        public ReturnableExitCodeAttribute(string description)
        {
            Description = description;
        }
    }
}
