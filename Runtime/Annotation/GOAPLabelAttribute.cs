using System;
namespace Kurisu.GOAP
{
    /// <summary>
    /// Label goals, actions or sensors
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class GOAPLabelAttribute : Attribute
    {
        public string Title=>mTitle;
		private readonly string mTitle;
        public GOAPLabelAttribute(string tite)
        {
            this.mTitle=tite;
        }
    }
}