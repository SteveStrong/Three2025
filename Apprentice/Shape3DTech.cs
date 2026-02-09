#nullable enable
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;

using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

using FoundryWorldsAndDrawings.ThreeD.Maths;


namespace Three2025.Apprentice;

public class Shape3DTech : IShape3DTech
{

   private IWorkspace Workspace;
   private IFoundryService FoundryService;
   private IShape3DEditor ShapeEditor;

   private FoStage3D? Stage { get; set; }


   private MockDataGenerator DataGenerator { get; set; } = new();



   public Shape3DTech(IWorkspace workspace, IFoundryService foundryService)
   {
      Workspace = workspace;
      FoundryService = foundryService;
      ShapeEditor = new Shape3DEditor(foundryService); // Create editor dynamically
   }

   /// <summary>
   /// Set the stage to use. Call this from a page to inject its stage.
   /// </summary>
   public void SetStage(FoStage3D stage)
   {
      Stage = stage;
      ShapeEditor.SetStage(stage);
   }

   [Description("Send a message to refresh the TreeView")]
   public void RefreshUI()
   {
      try
      {
         $"📢 Publishing RefreshRenderMessage to trigger re-render...".WriteInfo();
         FoundryService.PubSub().Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());
         $"✅ UI Refresh message published successfully".WriteSuccess();
      }
      catch (Exception ex)
      {
         $"❌ RefreshUI FAILED: {ex.Message}".WriteError();
         $"🔍 Stack trace: {ex.StackTrace}".WriteWarning();
      }
   }

   [Description("Establish a Geometry Stage for managing 3D shapes in the application")]
   public OPResult EstablishGeometryStage(string? stageName = null)
   {

      if (Stage != null)
      {
         // Ensure ShapeEditor always has the stage, even on repeated calls
         ShapeEditor.SetStage(Stage);
         return new OPResult("EstablishGeometryStage", ResultStatus.Stage3D, Stage);
      }

      var arena = Workspace.GetArena();

      // If stage name is specified, use it; otherwise use "AgentCanvas3D" as default
      var stageToUse = stageName ?? "AgentCanvas3D";
      Stage = arena.EstablishStage<FoStage3D>(stageToUse);

      // Connect ShapeEditor to this stage
      ShapeEditor.SetStage(Stage);

      $"Shape3DTech: Connected to stage '{stageToUse}'".WriteInfo();

      RefreshUI();
      return new OPResult("EstablishGeometryStage", ResultStatus.Stage3D, Stage);
   }

   [Description("Clears all shapes from the geometry stage")]
   public void ClearShapes()
   {
      ShapeEditor.ClearShapes();
   }

   [Description("Saves all shapes to a file for persistence")]
   public void SaveShapes()
   {
      var result = ShapeEditor.GetAllShapes();
      if (result.IsError())
      {
         $"❌ Failed to get shapes: {result.Display()}".WriteError();
         return;
      }
      // Use Value() method and cast to List<FoShape3D>
      var shapeList = result.Value() as List<FoShape3D> ?? new List<FoShape3D>();
      var shapes = shapeList.ToList(); // Now work with all FoShape3D objects
      // TODO: Serialization APIs removed — CodingExtensions, FileHelpers no longer available
      // var data = CodingExtensions.DehydrateList<FoShape3D>(shapes, false);
      // FileHelpers.WriteData("Data", "shapes.json", data);
   }

   [Description("Restores shapes from a saved file")]
   public void RestoreShapes()
   {
      // TODO: Serialization APIs removed — CodingExtensions, FileHelpers no longer available
      // var data = FileHelpers.ReadData("Data", "shapes.json");
      // var list = CodingExtensions.HydrateList<FoShape3D>(data, false);
      // ShapeEditor.ClearShapes();
      // foreach (var item in list)
      // {
      //    ShapeEditor.AddShape(item);
      // }
   }

   [Description("Generate a Random Color")]
   public OPResult PickARandomColor()
   {
      var color = DataGenerator.RandomColor();
      return new OPResult("PickARandomColor", ResultStatus.String, color);
   }

 
   [Description("Gets a list of all shapes and their current state")]
   public OPResult GetShapes()
   {
      return ShapeEditor.GetAllShapes();
   }

   [Description("Gets information about a specific shape by name")]
   public OPResult GetShapeByName(
      [Description("The name of the shape to query")] string name)
   {
      return ShapeEditor.GetShapeByName(name);
   }

   /// <summary>
   /// Create a 3D shape using the same factory logic as GeometryShape but without the wrapper class
   /// </summary>
   private FoShape3D CreateShapeWithFactory(string name, string shapeType, double width = 2.0, double height = 2.0, double depth = 2.0)
   {
      // Create a base shape to use factory methods on
      var factoryShape = new FoShape3D("factory");
      
      // Switch expression to select factory method (same as GeometryShape)
      Func<string, double, double, double, FoShape3D> factory = shapeType.ToLower() switch
      {
         "box" => factoryShape.CreateBox,
         "sphere" => factoryShape.CreateSphere,
         "cylinder" => factoryShape.CreateCylinder,
         "cone" => factoryShape.CreateCone,
         "torus" => factoryShape.CreateTorus,
         "tetrahedron" => factoryShape.CreateTetrahedron,
         "octahedron" => factoryShape.CreateOctahedron,
         "dodecahedron" => factoryShape.CreateDodecahedron,
         "icosahedron" => factoryShape.CreateIcosahedron,
         "torusknot" => factoryShape.CreateTorusKnot,
         "capsule" => factoryShape.CreateCapsule,
         "plane" => factoryShape.CreatePlane,
         "circle" => factoryShape.CreateCircle,
         "ring" => factoryShape.CreateRing,
         _ => factoryShape.CreateBox // default
      };
      
      var shape = factory(name, width, height, depth);
      
      // Set the spatial formatter (same as GeometryShape.DefaultFormatter)
      MxObject.SetCustomTreeViewNodeTitleFunction(shape, TreeNodeFormatters.Spatial);
      
      // Add the text tag (same as GeometryShape)
      var tag = new FoText3D("tag")
      {
         Text = name,
         FontSize = 0.5,
         Transform = new Transform3("TagTransform")
         {
            Position = new Vector3(3, 0, 0),
         },
         Color = "black"
      };
      shape.AddShape(tag);
      
      return shape;
   }

   [Description("Create and add a 3D shape to the geometry stage")]
   public OPResult AddShape(
      [Description("The name of the shape to create")] string name,
      [Description("The color of the shape")] string color,
      [Description("The type of shape: box, sphere, cylinder, cone, torus, tetrahedron, octahedron, dodecahedron, icosahedron, torusknot, capsule, plane, circle, ring")] string shapeType = "box",
      [Description("X coordinate position (optional, defaults to 0)")] double x = 0.0,
      [Description("Y coordinate position (optional, defaults to 0)")] double y = 0.0,
      [Description("Z coordinate position (optional, defaults to 0)")] double z = 0.0)
   {
      var newShape = CreateShapeWithFactory(name, shapeType);
      newShape.Color = color;

      // Set position if not at origin
      if (x != 0.0 || y != 0.0 || z != 0.0)
      {
         if (newShape.Transform == null)
         {
            newShape.Transform = new Transform3($"{name}_Transform");
         }
         newShape.Transform.Position = new Vector3(x, y, z);
      }

      ShapeEditor.AddShape(newShape);

      if (x != 0.0 || y != 0.0 || z != 0.0)
      {
         $"✅ Created {shapeType} '{name}' with color '{color}' at ({x:F1}, {y:F1}, {z:F1})".WriteSuccess();
      }
      else
      {
         $"✅ Created {shapeType} '{name}' with color '{color}'".WriteSuccess();
      }

      return new OPResult("AddShape", ResultStatus.Shape3D, newShape);
   }

   [Description("Create and add a 3D shape with specific dimensions")]
   public OPResult AddShapeWithDimensions(
      [Description("The name of the shape to create")] string name,
      [Description("The color of the shape")] string color,
      [Description("The type of shape")] string shapeType,
      [Description("Width (X dimension)")] double width,
      [Description("Height (Y dimension)")] double height,
      [Description("Depth (Z dimension)")] double depth,
      [Description("X coordinate position (optional, defaults to 0)")] double x = 0.0,
      [Description("Y coordinate position (optional, defaults to 0)")] double y = 0.0,
      [Description("Z coordinate position (optional, defaults to 0)")] double z = 0.0)
   {
      var newShape = CreateShapeWithFactory(name, shapeType, width, height, depth);
      newShape.Color = color;
      newShape.Transform = new Transform3($"{name}_Transform")
      {
         Position = new Vector3(x, y, z)
      };

      ShapeEditor.AddShape(newShape);

      $"✅ Created {shapeType} '{name}' ({width}x{height}x{depth}) with color '{color}' at ({x:F1}, {y:F1}, {z:F1})".WriteSuccess();

      return new OPResult("AddShapeWithDimensions", ResultStatus.Shape3D, newShape);
   }


   [Description("Delete a shape from the geometry stage")]
   public OPResult DeleteShape(
      [Description("The name of the shape to delete")] string name)
   {
      return ShapeEditor.DeleteShape(name);
   }

   [Description("Delete multiple shapes from the geometry stage")]
   public OPResult DeleteMultipleShapes(
      [Description("List of shape names to delete")] List<string> names)
   {
      return ShapeEditor.DeleteMultipleShapes(names);
   }

   [Description("Changes the X, Y, Z position of a shape in 3D space")]
   public OPResult RepositionShape(
      [Description("The name of the shape to reposition")] string name,
      [Description("The X coordinate")] double x,
      [Description("The Y coordinate")] double y,
      [Description("The Z coordinate")] double z)
   {
      return ShapeEditor.SetPosition(name, new Vector3(x, y, z));
   }

   [Description("Moves a shape by a relative offset (delta) from its current position")]
   public OPResult MoveShapeBy(
      [Description("The name of the shape to move")] string name,
      [Description("Offset to add to current X position")] double deltaX,
      [Description("Offset to add to current Y position")] double deltaY,
      [Description("Offset to add to current Z position")] double deltaZ)
   {
      return ShapeEditor.MoveBy(name, deltaX, deltaY, deltaZ);
   }

   [Description("Animates a shape moving by a relative offset (delta) from its current position")]
   public async Task<OPResult> AnimateMoveShapeBy(
      [Description("The name of the shape to move")] string name,
      [Description("Offset to add to current X position")] double deltaX,
      [Description("Offset to add to current Y position")] double deltaY,
      [Description("Offset to add to current Z position")] double deltaZ,
      [Description("Duration of animation in seconds")] float durationSeconds = 1.0f,
      [Description("Easing function: Linear, Quad, Cubic, Quart, Quint, Sine, Expo, Circ, Elastic, Back, Bounce (with In/Out/InOut variants like 'QuadInOut')")] string easing = "QuadInOut")
   {
      var result = GetShapeByName(name);
      if (result.IsError())
         return result; // Return the error OPResult
         
      var shape = result.AsShape3D();
      if (shape == null)
         return OPResult.Error($"Shape '{name}' could not be retrieved");

      var startPos = shape.Transform.Position;
      var endPos = new Vector3(startPos.X + deltaX, startPos.Y + deltaY, startPos.Z + deltaZ);
      
      // Create a temporary Vector3 to tween (not the actual shape position)
      var currentPos = new Vector3(startPos.X, startPos.Y, startPos.Z);
      var completed = false;
      
      // Use Unglide tweening library for smooth animation
      var tween = Unglide.Tween.TweenerImpl.Tweener.Tween(
         currentPos,
         new { X = endPos.X, Y = endPos.Y, Z = endPos.Z },
         durationSeconds
      );
      
      // Apply easing function
      tween.Ease(GetEasingFunction(easing));
      
      // OnUpdate: Push the tweened position through ShapeEditor to properly mark shape as stale
      tween.OnUpdate(_ => 
      {
         ShapeEditor.SetPosition(name, currentPos);
      });
      
      tween.OnComplete(() => completed = true);
      
      // Update loop - pump the tweener until animation completes
      var elapsed = 0f;
      var frameTime = 1f / 60f; // ~60 FPS
      
      while (!completed && elapsed < durationSeconds + 0.5f)
      {
         await Task.Delay((int)(frameTime * 1000));
         Unglide.Tween.TweenerImpl.Tweener.Update(frameTime);
         elapsed += frameTime;
      }
      
      // Ensure final position is exact
      var finalResult = ShapeEditor.SetPosition(name, endPos);
      return finalResult.IsError() 
         ? finalResult 
         : OPResult.Success($"Animated '{name}' to ({endPos.X:F1}, {endPos.Y:F1}, {endPos.Z:F1}) with {easing} easing");
   }

   /// <summary>
   /// Get the easing function by name from Unglide library
   /// </summary>
   private Func<float, float> GetEasingFunction(string easing)
   {
      return easing.ToLower() switch
      {
         "linear" => t => t, // Linear is just identity function
         "quadin" => Unglide.Ease.QuadIn,
         "quadout" => Unglide.Ease.QuadOut,
         "quadinout" => Unglide.Ease.QuadInOut,
         "cubicin" => Unglide.Ease.CubeIn,
         "cubicout" => Unglide.Ease.CubeOut,
         "cubicinout" => Unglide.Ease.CubeInOut,
         "quartin" => Unglide.Ease.QuartIn,
         "quartout" => Unglide.Ease.QuartOut,
         "quartinout" => Unglide.Ease.QuartInOut,
         "quintin" => Unglide.Ease.QuintIn,
         "quintout" => Unglide.Ease.QuintOut,
         "quintinout" => Unglide.Ease.QuintInOut,
         "sinein" => Unglide.Ease.SineIn,
         "sineout" => Unglide.Ease.SineOut,
         "sineinout" => Unglide.Ease.SineInOut,
         "expoin" => Unglide.Ease.ExpoIn,
         "expoout" => Unglide.Ease.ExpoOut,
         "expoinout" => Unglide.Ease.ExpoInOut,
         "circin" => Unglide.Ease.CircIn,
         "circout" => Unglide.Ease.CircOut,
         "circinout" => Unglide.Ease.CircInOut,
         "elasticin" => Unglide.Ease.ElasticIn,
         "elasticout" => Unglide.Ease.ElasticOut,
         "elasticinout" => Unglide.Ease.ElasticInOut,
         "backin" => Unglide.Ease.BackIn,
         "backout" => Unglide.Ease.BackOut,
         "backinout" => Unglide.Ease.BackInOut,
         "bouncein" => Unglide.Ease.BounceIn,
         "bounceout" => Unglide.Ease.BounceOut,
         "bounceinout" => Unglide.Ease.BounceInOut,
         _ => Unglide.Ease.QuadInOut // default
      };
   }

   [Description("Rotates a shape around X, Y, Z axes in degrees")]
   public OPResult RotateShape(
      [Description("The name of the shape to rotate")] string name,
      [Description("Rotation around X axis in degrees")] double xDegrees,
      [Description("Rotation around Y axis in degrees")] double yDegrees,
      [Description("Rotation around Z axis in degrees")] double zDegrees)
   {
      return ShapeEditor.RotateBy(name, xDegrees, yDegrees, zDegrees);
   }

   [Description("Scales a shape by multiplying its size on X, Y, Z axes")]
   public OPResult ScaleShape(
      [Description("The name of the shape to scale")] string name,
      [Description("Scale factor for X axis (1.0 = original size)")] double scaleX,
      [Description("Scale factor for Y axis (1.0 = original size)")] double scaleY,
      [Description("Scale factor for Z axis (1.0 = original size)")] double scaleZ)
   {
      return ShapeEditor.SetScale(name, new Vector3(scaleX, scaleY, scaleZ));
   }

   [Description("Changes the dimensions (width, height, depth) of an existing shape")]
   public OPResult ChangeShapeDimensions(
      [Description("The name of the shape")] string name,
      [Description("New width (X dimension)")] double width,
      [Description("New height (Y dimension)")] double height,
      [Description("New depth (Z dimension)")] double depth)
   {
      return ShapeEditor.SetDimensions(name, width, height, depth);
   }



   [Description("Changes the color of a shape")]
   public OPResult ChangeColor(
      [Description("The name of the shape")] string name,
      [Description("The new color for the shape")] string color)
   {
      var result = ShapeEditor.SetColor(name, color);
      $"🎨 ChangeColor returning: Name='{result.Name}', Status='{result.GetStatus()}', IsError={result.IsError()}".WriteInfo();
      return result;
   }

   [Description("Changes the geometry type of an existing shape")]
   public OPResult ChangeGeometry(
      [Description("The name of the shape")] string name,
      [Description("The new geometry type: box, sphere, cylinder, cone, torus, tetrahedron, octahedron, dodecahedron, icosahedron, torusknot, capsule, plane, circle, ring")] string shapeType,
      [Description("Optional new width (X dimension)")] double? width = null,
      [Description("Optional new height (Y dimension)")] double? height = null,
      [Description("Optional new depth (Z dimension)")] double? depth = null)
   {
      return ShapeEditor.SetGeometry(name, shapeType, width, height, depth);
   }

   [Description("Establish a text label on a shape (creates if missing, updates if exists)")]
   public OPResult EstablishTextLabel(
      [Description("The name of the parent shape")] string parentShapeName,
      [Description("The name for the label")] string labelName,
      [Description("The text to display")] string text,
      [Description("X position relative to parent (optional)")] double? relativeX = null,
      [Description("Y position relative to parent (optional, defaults to 2)")] double? relativeY = null,
      [Description("Z position relative to parent (optional)")] double? relativeZ = null,
      [Description("Font size (optional, defaults to 0.5)")] double? fontSize = null,
      [Description("Text color (optional, defaults to 'black')")] string? color = null)
   {
      Vector3? position = null;
      if (relativeX.HasValue || relativeY.HasValue || relativeZ.HasValue)
      {
         position = new Vector3(relativeX ?? 0, relativeY ?? 2, relativeZ ?? 0);
      }

      return ShapeEditor.EstablishTextLabel(parentShapeName, labelName, text, position, fontSize, color);
   }

   [Description("Remove a child shape from its parent shape")]
   public OPResult RemoveChildShape(
      [Description("The name of the parent shape")] string parentShapeName,
      [Description("The name of the child shape to remove")] string childShapeName)
   {
      return ShapeEditor.RemoveChildShape(parentShapeName, childShapeName);
   }

   [Description("Get the list of child shapes for a parent shape")]
   public OPResult GetChildShapes(
      [Description("The name of the parent shape")] string parentShapeName)
   {
      var result = ShapeEditor.GetChildShapes(parentShapeName);

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"📋 {result.Display()}".WriteInfo();
      }

      return result;
   }

   [Description("Create a link shape (IBodyLink3D) with dynamic geometry type support (Pipe/Tube/Line)")]
   public OPResult CreateLinkShape(
      [Description("The name for the link")] string name,
      [Description("The color of the link")] string color,
      [Description("The starting body shape")] FoShape3D fromShape,
      [Description("The ending body shape")] FoShape3D toShape,
      [Description("The radius of the link")] double radius,
      [Description("Geometry type: Pipe, Tube, or Line")] string geomType = "Pipe")
   {
      var link = new LinkShape(name, color, geomType)
      {
         FromShape3D = fromShape,
         ToShape3D = toShape,
         Radius = radius
      };

      ShapeEditor.AddShape(link);

      $"✅ Created link '{name}' [{geomType}] connecting '{fromShape.GetName()}' to '{toShape.GetName()}'".WriteSuccess();
      $"   📍 Link is IBodyLink3D (dependent shape) - renders AFTER bodies".WriteInfo();

      return new OPResult("CreateLinkShape", ResultStatus.Shape3D, link);
   }

   [Description("Change the geometry type of a link shape (Pipe/Tube/Line)")]
   public OPResult ChangeLinkGeometryType(
      [Description("The name of the link to modify")] string name,
      [Description("New geometry type: Pipe, Tube, or Line")] string geomType)
   {
      var result = ShapeEditor.GetShapeByName(name);

      if (result.IsError())
         return result;

      var link = result.AsShape3D() as LinkShape;
      if (link == null)
         return OPResult.Error($"Shape '{name}' is not a LinkShape");

      link.SetLinkGeometry(geomType);

      $"✅ Changed '{name}' geometry to {geomType}".WriteSuccess();

      return new OPResult("ChangeLinkGeometryType", ResultStatus.Shape3D, link);
   }

   [Description("Create a pipe link shape (IBodyLink3D) connecting two body shapes")]
   public OPResult CreatePipeLink(
      [Description("The name for the pipe link")] string name,
      [Description("The color of the pipe")] string color,
      [Description("The starting body shape")] FoShape3D fromShape,
      [Description("The ending body shape")] FoShape3D toShape,
      [Description("The radius of the pipe")] double radius)
   {
      var pipe = new FoPipe3D(name, color)
      {
         FromShape3D = fromShape,
         ToShape3D = toShape
      };
      pipe.CreatePipe(name, radius);

      ShapeEditor.AddShape(pipe);

      $"✅ Created pipe link '{name}' connecting '{fromShape.GetName()}' to '{toShape.GetName()}'" .WriteSuccess();
      $"   📍 Pipe is IBodyLink3D (dependent shape) - renders AFTER bodies".WriteInfo();

      return new OPResult("CreatePipeLink", ResultStatus.Shape3D, pipe);
   }

   [Description("Create a pathway link shape (IBodyLink3D) connecting two body shapes")]
   public OPResult CreatePathwayLink(
      [Description("The name for the pathway link")] string name,
      [Description("The starting body shape")] FoShape3D fromShape,
      [Description("The ending body shape")] FoShape3D toShape)
   {
      var pathway = new FoPathway3D(name)
      {
         FromShape3D = fromShape,
         ToShape3D = toShape
      };

      ShapeEditor.AddShape(pathway);

      $"✅ Created pathway link '{name}' connecting '{fromShape.GetName()}' to '{toShape.GetName()}'".WriteSuccess();
      $"   📍 Pathway is IBodyLink3D (dependent shape) - renders AFTER bodies".WriteInfo();

      return new OPResult("CreatePathwayLink", ResultStatus.Shape3D, pathway);
   }


}
