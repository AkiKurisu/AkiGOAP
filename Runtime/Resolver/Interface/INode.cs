/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public interface INode
    {
        GOAPState[] Effects { get; }
        GOAPState[] Conditions { get; }
    }
}
