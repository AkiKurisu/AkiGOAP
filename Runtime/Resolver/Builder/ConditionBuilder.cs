using System.Collections.Generic;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class ConditionBuilder : IConditionBuilder
    {
        private readonly List<GOAPState> conditionIndexList;
        private readonly bool[] conditionsMetList;

        public ConditionBuilder(List<GOAPState> conditionIndexList)
        {
            this.conditionIndexList = conditionIndexList;
            conditionsMetList = new bool[this.conditionIndexList.Count];
        }

        public IConditionBuilder SetConditionMet(GOAPState condition, bool met)
        {
            var index = conditionIndexList.IndexOf(condition);

            if (index == -1)
                return this;

            conditionsMetList[index] = met;

            return this;
        }

        public bool[] Build()
        {
            return conditionsMetList;
        }

        public void Clear()
        {
            for (int i = 0; i < conditionIndexList.Count; i++) conditionsMetList[i] = false;
        }
    }
}