using Unity.Mathematics;
using UnityEngine;
namespace Kurisu.GOAP.Resolver
{
    public interface IPositionBuilder
    {
        IPositionBuilder SetPosition(INode node, Vector3 position);
        float3[] Build();
        void Clear();
    }
}