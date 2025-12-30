using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Maths;

#nullable enable




public interface ICageTech : ITechnician
{
    void SetStage(FoStage3D stage);
    void CreateRoutingCage();
    void CreateCageForRack(string name);
    (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section);
}

public class CageTech : ICageTech
{
    protected IWorkspace Workspace { get; init; }
    protected IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();
    protected FoStage3D? Stage { get; set; }



    public CageTech(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        FoundryService = foundry;
    }

    /// <summary>
    /// Set the stage to use. Call this from a page to inject its stage.
    /// </summary>
    public void SetStage(FoStage3D stage)
    {
        Stage = stage;
    }

    private FoStage3D GetStage()
    {
        if (Stage != null) return Stage;
        var arena = Workspace.GetArena();
        return arena.EstablishStage<FoStage3D>("Cage");
    }

    public void CreateRoutingCage()
    {
        var trayNodes = new List<Node3D>();

        var stage = GetStage();

        var racks = stage.GetMembers<FoRack>();
        foreach (var rack in racks ?? Enumerable.Empty<FoRack>())
        {
            var nodes = CreateCageForRack(rack);
            trayNodes.AddRange(nodes);
        }

        AddLinksBetweenTrays(stage, trayNodes, "Aqua");
    }

    private void AddLinksBetweenTrays(FoStage3D stage, List<Node3D> nodes, string color)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            var start = nodes[i - 1];
            var finish = nodes[i];

            if ( start.GetParent() == finish.GetParent() ) 
                continue;

            var link = new Link3D($"Link:{start.GetTitle()}->{finish.GetTitle()}", color, start, finish);
            
            stage.AddShape(link);

            start.AddLink(link);
            finish.AddLink(link);

            $"{link.GetName()} distance {link.Distance()}".WriteSuccess();
        }
    }

    public void CreateCageForRack(string name)
    {

        var stage = GetStage();

        var (success, rack) = stage.FindMember<FoRack>(name);
        if (!success) {
            FoundryService.Toast().Error($"Rack {name} not found");
            return;
        }
        CreateCageForRack(rack);
    }

    public List<Node3D> CreateCageForRack(FoRack rack)
    {
        var trayNodes = new List<Node3D>();
        var allNodes = new List<Node3D>();

        var stage = GetStage();

        
        var equip = rack.GetEquipment();
        foreach (var item in equip)
        {
            var equipCage = CreateCageForEquipment(item);
            allNodes.AddRange(equipCage);
        }

        //add the tray at top of rack
        var trays = rack.GetTrays();
        foreach (var item in trays)
        {
            var trayCage = CreateCageForTray(item);
            trayNodes.AddRange(trayCage);
        }

        allNodes.AddRange(trayNodes);

        //get the nodes for the rack left rail
        var leftRail = allNodes.Where(p => p.GetName().Matches("L")).OrderBy(p => p.GetPosition().Y).ToList();
        AddLinksBetween(rack, stage, leftRail, "Blue");

        var rightRail = allNodes.Where(p => p.GetName().Matches("R")).OrderBy(p => p.GetPosition().Y).ToList();
        AddLinksBetween(rack, stage, rightRail, "Blue");

        return trayNodes;

    }

    public List<Node3D> CreateCageForEquipment(FoEquipment equip)
    {
        var stage = GetStage();

        var nodes = new List<Node3D>();

        var connections = equip.GetConnectors();
        foreach (var item in connections)
        {
            //var (success, data) = item.GetComputedMesh();
            if (!item.IsWorldPositionUpdateRequired()) continue;
            var (found, pos) = item.GetWorldPosition();
            if ( !found ) continue;

            var node = new Node3D(item.GetName(), "Blue")
            {
                Transform = new Transform3("NodeTransform")
                {
                    Position = pos,
                }
            };
            node.CreateBox(item.GetName(), .2, .2, .3);
            //arena.AddShapeToStage<Node3D>(cage);
            equip.AddShape<Node3D>(node);
            nodes.Add(node);
        }

        AddLinksBetween(equip, stage, nodes, "Blue");

        return nodes;
    }

    public List<Node3D> CreateCageForTray(FoTray tray)
    {
        var stage = GetStage();

        var nodes = new List<Node3D>();

        var connections = tray.GetConnectors();
        foreach (var item in connections)
        {
            //var (success, data) = item.GetComputedMesh();
            if (!item.IsWorldPositionUpdateRequired()) continue;
            var (found, pos) = item.GetWorldPosition();
            if ( !found ) continue;

            var node = new Node3D(item.GetName(), "Blue")
            {
                Transform = new Transform3("NodeTransform")
                {
                    Position = pos,
                }
            };
            node.CreateSphere(item.GetName(), 0.3, 0.3, 0.3);
            tray.AddShape<Node3D>(node);
            //arena.AddShapeToStage<Node3D>(cage);
            nodes.Add(node);
        }

        AddLinksBetween(tray, stage, nodes, "Blue");

        return nodes;
    }



    private void AddLinksBetween(FoShape3D parent, FoStage3D stage, List<Node3D> nodes, string color)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            var start = nodes[i - 1];
            var finish = nodes[i];
            var link = new Link3D($"Link:{parent.GetName()}:{start.GetTitle()}->{finish.GetTitle()}", color, start, finish);
            stage.AddShape(link);

            start.AddLink(link);
            finish.AddLink(link);

            //$"{link.GetName()} distance {link.Distance()}".WriteSuccess();
        }
    }






    public (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section)
    {

        var root = new FoShape3D(name)
        {
            Transform = new Transform3("BoxTransform")
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateBoundary(name, 10, 10, 10);


        var outerBox = new SpacialBox3D(10, 10, 10, "cm");
        var innerBox = new SpacialBox3D(10.5, 8, 8, "cm");

        var leftFace = innerBox.GetLocalLeftFace();
        var rightFace = innerBox.GetLocalRightFace();


        DrawPipe(root, "leftedge", "red", leftFace.Vertices);
        DrawPipe(root, "rightedge", "green", rightFace.Vertices);

        
        leftFace = outerBox.GetLocalLeftFace();
        rightFace = outerBox.GetLocalRightFace();
        DrawFace(root, "Left", leftFace.FaceMesh("blue", .8));
        DrawFace(root, "Right", rightFace.FaceMesh("yellow", .8));

        return (i,root);
    }

    private static FoGlyph3D DrawFace(FoShape3D root, string name, Mesh3D face)
    {
        var shape = new FoGlyph3D(name, face);
        root.AddShape(shape);
        return shape;
    }

    private static FoPipe3D DrawPipe(FoShape3D root, string name,  string color, List<Point3D> points)
    {
        var path = points.Select(p => p.AsVector3()).ToList();
        var pipe = new FoPipe3D(name)
        {
            Color = color,
            Closed = true,
        };
        pipe.CreateTube(name, 0.1, path);

        root.AddShape(pipe);
        return pipe;
    }



}