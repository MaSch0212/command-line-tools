using MaSch.Core;
using System;

namespace MaSch.CommandLineTools.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CltToolAttribute : Attribute
    {
        public string? CreatorMethodName { get; }
        public string? WriteExitCodesMethodName { get; }

        public CltToolAttribute(string? creatorMethodName, string? writeExitCodesMethodName)
        {
            CreatorMethodName = creatorMethodName;
            WriteExitCodesMethodName = writeExitCodesMethodName;
        }
    }
}
