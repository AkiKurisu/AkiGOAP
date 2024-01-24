using System;
namespace Kurisu.GOAP
{
    /// <summary>
    /// To be categorized in the search window, and can be sub-categorized with the '/' symbol
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GOAPGroupAttribute : Attribute
    {
        public string Group => mGroup;
        private readonly string mGroup;
        public GOAPGroupAttribute(string group)
        {
            mGroup = group;
        }
    }
}