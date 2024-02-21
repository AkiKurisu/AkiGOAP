using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.GOAP
{
    public abstract class BaseBackend : IDisposable, IBackend
    {
        protected const int Capacity = 10;
        #region  Host Status
        protected bool IsActive { get => backendHost.IsActive; set => backendHost.IsActive = value; }
        protected WorldState WorldState => backendHost.WorldState;
        protected LogType LogType => backendHost.LogType;
        protected TickType TickType => backendHost.TickType;
        protected SearchMode SearchMode => backendHost.SearchMode;
        #endregion
        public IGoal ActivateGoal { get; protected set; }
        public virtual int ActiveActionIndex { get; }
        public virtual List<IAction> ActivatePlan { get; }
        protected bool CanTickPlan => !TickType.HasFlag(TickType.ManualTickPlan);
        #region  Log
        protected bool LogActive => LogType.HasFlag(LogType.OnlyActive);
        protected bool LogSearch => LogType.HasFlag(LogType.IncludeSearch);
        protected bool LogFail => LogType.HasFlag(LogType.IncludeFail);
        #endregion
        public List<IAction> Actions { get; } = new(Capacity);
        public List<IGoal> Goals { get; } = new(Capacity);
        public abstract void ManualActivate();
        public virtual void OnDisable() { }
        public virtual void LateUpdate() { }
        public abstract void Update();
        private readonly IBackendHost backendHost;
        public IBackendHost BackendHost => backendHost;
        public BaseBackend(IBackendHost backendHost)
        {
            this.backendHost = backendHost;
        }
        public void TickGoals()
        {
            for (int i = 0; i < Goals.Count; i++)
            {
                Goals[i].OnTick();
            }
        }
        protected void ActivePlanLog(object message, bool bold = false)
        {
            string s = $"<color=#5BDB14>ActivePlan: {message}</color>";
            if (bold)
            {
                s = "<b>" + s + "</b>";
            }
            Debug.Log(s, BackendHost.Transform.gameObject);
        }

        protected void PlannerLog(object message, bool bold = false)
        {
            string s = $"<color=#00C2FF>Planner: {message}</color>";
            if (bold)
            {
                s = "<b>" + s + "</b>";
            }
            Debug.Log(s, BackendHost.Transform.gameObject);
        }
        public void InjectGoals(IEnumerable<IGoal> source)
        {
            Goals.Clear();
            foreach (var goal in source)
            {
                Goals.Add(goal);
                goal.Init(WorldState);
            }
            OnGoalInjected();
        }
        protected virtual void OnGoalInjected() { }
        public void InjectActions(IEnumerable<IAction> source)
        {
            Actions.Clear();
            foreach (var action in source)
            {
                Actions.Add(action);
                action.Init(WorldState);
            }
            OnActionInjected();
        }
        protected virtual void OnActionInjected() { }
        protected void NotifyHostUpdate()
        {
            backendHost.NotifyUpdate();
        }
        protected bool NoActiveGoal()
        {
            return ActivateGoal == null;
        }

        public virtual void Dispose() { }
        public abstract void AbortActivePlan();
        public abstract void CleanUp();
    }
}
