using UnityEngine;
namespace Kurisu.GOAP
{
    public interface IBackendHost
    {
        void NotifyUpdate();
        bool IsActive { get; set; }
        SearchMode SearchMode { get; }
        LogType LogType { get; }
        TickType TickType { get; }
        WorldState WorldState { get; }
        Transform Transform { get; }
    }
}