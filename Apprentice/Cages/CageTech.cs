using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorThreeJS.Core;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Solutions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Units;
using Three2025.Apprentice;




public interface ICageTech : ITechnician
{
    void CreateRoutingCage();
    void CreateCageForRack(string name);
    (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section);
}

public class CageTech : ICageTech
{
    protected IWorkspace Workspace { get; init; }
    protected IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();



    public CageTech(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        FoundryService = foundry;
    }

    public void CreateRoutingCage()
    {
        var trayNodes = new List<Node3D>();

        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        var racks = stage.GetMembers<FoRack>();
        foreach (var rack in racks)
        {
            var nodes = CreateCageForRack(rack);
            trayNodes.AddRange(nodes);
        }

        AddLinksBetweenTrays(arena, trayNodes, "Aqua");
    }

    private void AddLinksBetweenTrays(IArena arena, List<Node3D> nodes, string color)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            var start = nodes[i - 1];
            var finish = nodes[i];

            if ( start.GetParent() == finish.GetParent() ) 
                continue;

            var link = new Link3D($"Link:{start.GetTitle()}->{finish.GetTitle()}", color, start, finish);
            arena.AddShapeToStage<Link3D>(link);

            start.AddLink(link);
            finish.AddLink(link);

            $"{link.GetName()} distance {link.Distance()}".WriteSuccess();
        }
    }

    public void CreateCageForRack(string name)
    {

        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

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

        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        
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
        AddLinksBetween(rack, arena, leftRail, "Blue");

        var rightRail = allNodes.Where(p => p.GetName().Matches("R")).OrderBy(p => p.GetPosition().Y).ToList();
        AddLinksBetween(rack, arena, rightRail, "Blue");

        return trayNodes;

    }

    public List<Node3D> CreateCageForEquipment(FoEquipment equip)
    {
        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        var nodes = new List<Node3D>();

        var connections = equip.GetConnectors();
        foreach (var item in connections)
        {
            var (success, data) = item.GetValue3D();
            if (!success || data.HitBoundary == null) continue;

            var node = new Node3D(item.GetName(), "Blue")
            {
                Transform = new Transform3()
                {
                    Position = data.HitBoundary.GetPosition(),
                }
            };
            node.CreateBox(item.GetName(), .2, .2, .3);
            //arena.AddShapeToStage<Node3D>(cage);
            equip.AddSubGlyph3D<Node3D>(node);
            nodes.Add(node);
        }

        AddLinksBetween(equip, arena, nodes, "Blue");

        return nodes;
    }

    public List<Node3D> CreateCageForTray(FoTray tray)
    {
        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        var nodes = new List<Node3D>();

        var connections = tray.GetConnectors();
        foreach (var item in connections)
        {
            var (success, data) = item.GetValue3D();
            if (!success || data.HitBoundary == null) continue;

            var node = new Node3D(item.GetName(), "Blue")
            {
                Transform = new Transform3()
                {
                    Position = data.HitBoundary.GetPosition(),
                }
            };
            node.CreateSphere(item.GetName(), 0.3, 0.3, 0.3);
            tray.AddSubGlyph3D<Node3D>(node);
            //arena.AddShapeToStage<Node3D>(cage);
            nodes.Add(node);
        }

        AddLinksBetween(tray, arena, nodes, "Blue");

        return nodes;
    }



    private void AddLinksBetween(FoShape3D parent, IArena arena, List<Node3D> nodes, string color)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            var start = nodes[i - 1];
            var finish = nodes[i];
            var link = new Link3D($"Link:{parent.GetName()}:{start.GetTitle()}->{finish.GetTitle()}", color, start, finish);
            arena.AddShapeToStage<Link3D>(link);

            start.AddLink(link);
            finish.AddLink(link);

            //$"{link.GetName()} distance {link.Distance()}".WriteSuccess();
        }
    }

    public bool ComputeHitBoundaries(Action OnComplete)
    {
        var arena = FoundryService.Arena();
        var (success, scene) = arena.CurrentScene();

        if (!success) return false;
        scene.UpdateHitBoundaries(OnComplete);
        return true;
    } 

 




    public (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section)
    {

        var root = new FoShape3D(name)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateBoundary(name, 10, 10, 10);


        var outerBox = new SpacialBox3D(10, 10, 10, "cm");
        var innerBox = new SpacialBox3D(10.5, 8, 8, "cm");

        var leftFace = innerBox.FaceCenters.FirstOrDefault(p => p.Name.Matches("left"));
        var rightFace = innerBox.FaceCenters.FirstOrDefault(p => p.Name.Matches("right"));


        var leftList = new List<Point3D>(innerBox.LeftFace) { leftFace };
        var rightlist = new List<Point3D>(innerBox.RightFace) { rightFace };


        DrawPipe(root, "leftedge", "red", innerBox.LeftFace);
        DrawPipe(root, "rightedge", "green", innerBox.RightFace);

        DrawFace(root, "Left", outerBox.LeftFaceMesh(0.1, "blue"));
        DrawFace(root, "Right", outerBox.RightFaceMesh(0.1, "yellow"));

        return (i,root);
    }

    private static FoGlyph3D DrawFace(FoShape3D root, string name, Mesh3D face)
    {
        var shape = new FoGlyph3D(name);
        shape.SetValue3D(face);
        root.AddSubGlyph3D(shape);
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

        root.AddSubGlyph3D(pipe);
        return pipe;
    }



}