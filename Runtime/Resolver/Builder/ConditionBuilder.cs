using System.Collections.Generic;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class ConditionBuilder:IConditionBuilder
    {
        private readonly List<GOAPState> conditionIndexList;
        private bool[] conditionsMetList;
        
        public ConditionBuilder(List<GOAPState> conditionIndexList)
        {
            this.conditionIndexList = conditionIndexList;
            this.conditionsMetList = new bool[this.conditionIndexList.Count];
        }
        
        public IConditionBuilder SetConditionMet(GOAPState condition, bool met)
        {
            var index = this.conditionIndexList.FindIndex(x => x == condition);

            if (index == -1)
                return this;
            
            this.conditionsMetList[index] = met;

            return this;
        }
        
        public bool[] Build()
        {
            return this.conditionsMetList;
        }
        
        public void Clear()
        {
            for(int i=0;i<conditionIndexList.Count;i++)this.conditionsMetList[i]=false;
        }
    }
}