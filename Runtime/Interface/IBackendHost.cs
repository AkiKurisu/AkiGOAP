using UnityEngine;
namespace Kurisu.GOAP
{
    public interface IBackendHost
    {
        void NotifyUpdate();
        bool SkipSearchWhenActionRunning { get; }
        bool IsActive { get; set; }
        LogType LogType { get; }
        TickType TickType { get; }
        WorldState WorldState { get; }
        Transform Transform { get; }
    }
}