

using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;


namespace Three2025.Model;
public class CableWorld : FoWorld3D
{
    public CableWorld(string name) : base(name)
    {
    }

    public override string GetTreeNodeTitle()
    {
        return $"CableWorld {base.GetTreeNodeTitle()}";
    }


}