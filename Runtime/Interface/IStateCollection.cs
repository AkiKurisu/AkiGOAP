using System.Collections.Generic;
namespace Kurisu.GOAP
{
    /// <summary>
    /// A collection of World states
    /// </summary>
    public interface IStateCollection
    {
        bool IsSubset(IEnumerable<KeyValuePair<string, bool>> state);
    }
}
