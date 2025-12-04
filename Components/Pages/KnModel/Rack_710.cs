using Blazor.Diagrams.Core.Controls;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

using FoundryRulesAndUnits.Units;


using FoundryRulesAndUnits.Models;
using Microsoft.Extensions.FileProviders;


namespace Plugin_710.Model;

public class Rack_710 : CircuitBlock_710
{

    public Rack_710(DT_Component component) : base(component)
    {
    }

    public Rack_710(DT_Component component, int index, Base_710 parent): this(component)
    {
        ItemIndex = index;
        SetKnParent(parent);
    }


    public override void BuildStructure(IEnumerable<DT_Component> members, Base_710 parent, Dictionary<string, Base_710> lookup)
    {
        var i = 0;
        var list = members.Where(x => x.IsEquipment()).ToList();
        foreach (var item in list)
        {
            var obj = FindOrBuild<Rack_710,Equipment_710>(this,item, i++,lookup);
            obj.BuildStructure(item.GetMembers(), this, lookup);
        }

        list = members.Where(x => x.IsTray()).ToList();
        foreach (var item in list)
        {
            var obj = FindOrBuild<Rack_710,Tray_710>(this,item, i++,lookup);
            obj.BuildStructure(item.GetMembers(), this, lookup);
        }
        //i = 0;
        list = members.Where(x => x.IsConnector()).ToList();
        foreach (var item in list)
        {
            var obj = FindOrBuild<Rack_710,Connector_710>(this,item, i++,lookup);
            obj.BuildStructure(item.GetMembers(), this, lookup);
        }

       // i = 0;
        list = members.Where(x => x.IsPanel()).ToList();
        foreach (var item in list)
        {
            var obj = FindOrBuild<Rack_710,Panel_710>(this,item, i++,lookup);
            obj.BuildStructure(item.GetMembers(), this, lookup);
        }

       // i = 0;
        list = members.Where(x => x.IsSubSystem()).ToList();
        foreach (var item in list)
        {
            var obj = FindOrBuild<Rack_710,SubSystem_710>(this,item, i++,lookup);
            obj.BuildStructure(item.GetMembers(), this, lookup);
        }
    }

    // public override KnEditor2DParameter EstablishEditor(string view, MentorDiagram diagram)
    // {
    //     var result = base.EstablishEditor(view, diagram);
    //     if ( result.IsValid())
    //         return result;     
              

    //     var node = EstablishDiagramGroup<CircuitBlockEditor>(result, diagram);


    //     // diagram.RegisterComponent<NodeInformationControl, NodeInformationControlWidget>(true);
    //     // diagram.Controls.AddFor(node, ControlsType.OnSelection).Add(new NodeInformationControl(this));

    //     return result;
    // } 

    // public override KnEditor2DParameter FinalizeEditor(string view)
    // {
    //     var result = base.FinalizeEditor(view);
    //     if ( !result.IsValid())
    //         return result;  

    //     var group = result.GetCurrentValueAs<CircuitBlockEditor>();

    //     var equipment = this.ModelComponents<Equipment_710>();
    //     foreach (var obj in equipment)
    //     {
    //         var item = obj.FinalizeEditor(view);
    //         //var (x1,y1) = item.GetPosition();
    //         //item.SetPosition((int)Location.X, (int)y1);

    //         var child = item.GetCurrentValueAs<CircuitBlockEditor>();
    //         group.AddChild(child);
    //     };

    //     var subsystem = this.ModelComponents<SubSystem_710>();
    //     foreach (var obj in subsystem)
    //     {
    //         var item = obj.FinalizeEditor(view);
    //         //var (x1,y1) = item.GetPosition();
    //         //item.SetPosition((int)Location.X, (int)y1);

    //         var child = item.GetCurrentValueAs<CircuitBlockEditor>();
    //         group.AddChild(child);
    //     };

    //     var connectors = this.ModelComponents<Connector_710>();
    //     foreach (var obj in connectors)
    //     {
    //         var item = obj.FinalizeEditor(view);
    //         //var (x1, y1) = item.GetPosition();
    //         //item.SetPosition((int)Location.X, (int)y1);
    //         var child = item.GetCurrentValueAs<ConnectorEditor>();
    //         group.AddChild(child);
    //     };

    //     var cable = Subcomponents<Cable_710>();
    //     foreach (var obj in cable)
    //     {
    //         var item = obj.FinalizeEditor(view);
    //         var child = item.GetCurrentValueAs<CableEditor>();
    //     };

    //     return result!;
    // } 

    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view, IArena? page)
    {
        var result = Compute3DGeometry(view, geom => 
        {
            geom.ApplyMethod("ComputeGeometry", ComputeShape3D, null, null); //, null, (p,v) => page.DeleteShape(v.AsGlyph2D()));
        });

        return (result, result.GetParameter());
    }



    //public override OPResult Geometry3DRender(string view, IArena arena, bool deep)
    //{
    //    var result = base.Geometry3DRender(view, arena, deep);

    //    if (result.IsSuccess())
    //    {
    //        var shape = result.AsShape3D();
    //        arena.AddShapeToStage<FoShape3D>(shape);
    //    }
    //    return result;
    //}

    private bool UpdateShape3D(KnGeometry geometry, FoShape3D shape)
    {
        if (geometry == null)
            return false;

        return true;
    }

    private bool ComputeShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null)
            return false;

        var shape = geometry.GetCashe<FoShape3D>();

        if ( geometry.IsCasheEmpty())
        {

            var model = context.GetKnParentOfType<Model_710>();
            if ( model == null)
                return false;

            var arena = model.GetArena();

            var title = context.Title ?? "";
            if ( context.GetParent() is Base_710 obj)
                title = obj.Source.GetPart().StructureReference ?? title;
            

            var spec = new FoSpec3D()
            {
                Name = context.GetName(),
                Title = title,
                Color = Source.MetaData().GetValue("Color", "white"),
                GlyphId = GetKnowId(),
                W = context.FindLengthValue("Width", 0).Value(),
                H = context.FindLengthValue("Height", 0).Value(),
                D = context.FindLengthValue("Depth", 0).Value(),
                X = context.FindLengthValue("X", 0).Value(),
                Y = context.FindLengthValue("Y", 0).Value(),
                Z = context.FindLengthValue("Z", 0).Value(),
                Px = context.FindLengthValue("PivotX", 0).Value(),
                Py = context.FindLengthValue("PivotY", 0).Value(),
                Pz = context.FindLengthValue("PivotZ", 0).Value(),
            };

            shape = FoRack.CreateRack(spec);

            geometry.SetCashe(shape);
        }
        else
        {
            UpdateShape3D(geometry, shape);
        }
    
        result.SetValue(ResultStatus.Shape3D, shape);

        return true;
    }

}
