#nullable enable

using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.PubSub;

namespace Three2025.Apprentice;

/// <summary>
/// Simple shape editor following ModelEditor pattern.
/// Changes publish RefreshUIEvent for tree updates.
/// </summary>
public class Shape3DEditor : IShape3DEditor
{
   private readonly IFoundryService _foundryService;
   private FoStage3D? _stage;

   public Shape3DEditor(IFoundryService foundryService)
   {
      _foundryService = foundryService;
   }

   public void SetStage(FoStage3D stage)
   {
      _stage = stage;
      $"🎬 ShapeEditor connected to stage: {stage.GetName()}".WriteInfo();
   }
   
   private void ShapeChanged()
   {
      _foundryService.PubSub().Publish(RefreshUIEvent.TreeView());
   }

   public OPResult SetColor(string shapeName, string color)
   {
      var result = FindShape(shapeName);
      if (result.IsError()) 
         return result;


      var shape = result.AsShape3D();
      shape.Color = color;
      ShapeChanged();
      return result;
   }

   public OPResult SetPosition(string shapeName, Vector3 position)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      EnsureTransform(shape, shapeName);
      shape.Transform!.Position = position;
      ShapeChanged();
      return result;
   }

   public OPResult SetRotation(string shapeName, Euler rotation)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      EnsureTransform(shape, shapeName);
      shape.Transform!.Rotation = rotation;
      ShapeChanged();
      return result;
   }

   public OPResult SetScale(string shapeName, Vector3 scale)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      EnsureTransform(shape, shapeName);
      shape.Transform!.Scale = scale;
      ShapeChanged();
      return result;
   }

   public OPResult SetGeometry(string shapeName, string shapeType, double? width = null, double? height = null, double? depth = null)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      
      // Use provided dimensions or current shape dimensions
      var w = width ?? shape.Width;
      var h = height ?? shape.Height;
      var d = depth ?? shape.Depth;

      // Switch expression to select factory method
      Func<string, double, double, double, FoShape3D> factory = shapeType.ToLower() switch
      {
         "box" => shape.CreateBox,
         "sphere" => shape.CreateSphere,
         "cylinder" => shape.CreateCylinder,
         "cone" => shape.CreateCone,
         "torus" => shape.CreateTorus,
         "tetrahedron" => shape.CreateTetrahedron,
         "octahedron" => shape.CreateOctahedron,
         "dodecahedron" => shape.CreateDodecahedron,
         "icosahedron" => shape.CreateIcosahedron,
         "torusknot" => shape.CreateTorusKnot,
         "capsule" => shape.CreateCapsule,
         "plane" => shape.CreatePlane,
         "circle" => shape.CreateCircle,
         "ring" => shape.CreateRing,
         _ => shape.CreateBox // default to box
      };

      factory(shapeName, w, h, d);
      ShapeChanged();
      return result;
   }

   public OPResult SetDimensions(string shapeName, double width, double height, double depth)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      shape.Width = width;
      shape.Height = height;
      shape.Depth = depth;
      ShapeChanged();
      return result;
   }

   public OPResult EstablishTextLabel(string parentShapeName, string labelName, string text, Vector3? relativePosition = null, double? fontSize = null, string? color = null)
   {
      var result = FindShape(parentShapeName);
      if (result.IsError())
         return result;

      var parentShape = result.AsShape3D();
      
      // Try to find existing label
      var existingChild = parentShape.FindSubGlyph3D<FoGlyph3D>(labelName);
      var existingLabel = existingChild as FoText3D;
      
      if (existingLabel != null)
      {
         // Update existing label
         existingLabel.Text = text;
         if (fontSize.HasValue)
            existingLabel.FontSize = fontSize.Value;
         if (color != null)
            existingLabel.Color = color;
         if (relativePosition != null && existingLabel.Transform != null)
            existingLabel.Transform.Position = relativePosition;
         
         ShapeChanged();
         return new OPResult("EstablishTextLabel", ResultStatus.Shape3D, existingLabel);
      }
      
      // Create new label
      var position = relativePosition ?? new Vector3(0, 2, 0);
      var newLabel = new FoText3D(labelName)
      {
         Text = text,
         FontSize = fontSize ?? 0.5,
         Transform = new Transform3($"{labelName}_Transform")
         {
            Position = position,
         },
         Color = color ?? "black"
      };
      
      parentShape.AddShape(newLabel);
      ShapeChanged();
      
      return new OPResult("EstablishTextLabel", ResultStatus.Shape3D, newLabel);
   }

   public OPResult RemoveChildShape(string parentShapeName, string childShapeName)
   {
      var result = FindShape(parentShapeName);
      if (result.IsError())
         return result;

      var parentShape = result.AsShape3D();
      
      var child = parentShape.FindSubGlyph3D<FoGlyph3D>(childShapeName);
      if (child == null)
      {
         return new OPResult("RemoveChildShape", ResultStatus.Error, $"Child shape '{childShapeName}' not found in parent '{parentShapeName}'");
      }
      
      parentShape.Remove<FoGlyph3D>(child);
      ShapeChanged();
      
      return new OPResult("RemoveChildShape", ResultStatus.Shape3D, $"Removed child shape '{childShapeName}' from parent '{parentShapeName}'");
   }

   public OPResult GetChildShapes(string parentShapeName)
   {
      var result = FindShape(parentShapeName);
      if (result.IsError())
         return result;

      var parentShape = result.AsShape3D();
      var children = parentShape.GetMembers<FoGlyph3D>();
      
      if (children == null || children.Count == 0)
      {
         return new OPResult("GetChildShapes", ResultStatus.String, $"Shape '{parentShapeName}' has no child shapes");
      }
      
      var childNames = children.Select(c => c.GetName()).ToList();
      var message = $"Shape '{parentShapeName}' has {childNames.Count} child shapes: {string.Join(", ", childNames)}";
      
      return new OPResult("GetChildShapes", ResultStatus.String, message);
   }

   public OPResult DeleteShape(string name)
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return new OPResult("DeleteShape", ResultStatus.Error, "No stage connected");
      }

      var (success, shape) = _stage.FindMember<FoGlyph3D>(name);
      var foShape = shape as FoShape3D;

      if (foShape != null)
      {
         foShape.DeleteFromStage(_stage);
         $"✅ Deleted shape '{name}'".WriteSuccess();
         ShapeChanged();
         return new OPResult("DeleteShape", ResultStatus.String, $"Successfully deleted shape '{name}'");
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
         return new OPResult("DeleteShape", ResultStatus.Error, $"Shape '{name}' not found");
      }
   }

   public OPResult DeleteMultipleShapes(List<string> names)
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return new OPResult("DeleteMultipleShapes", ResultStatus.Error, "No stage connected");
      }

      var deletedShapes = new List<string>();
      var failedShapes = new List<string>();

      foreach (var name in names)
      {
         var (success, found) = _stage.FindMember<FoGlyph3D>(name);
         var shape = found as FoShape3D;
         if (shape != null)
         {
            shape.DeleteFromStage(_stage);
            deletedShapes.Add(name);
         }
         else
         {
            failedShapes.Add(name);
         }
      }

      var message = $"Deleted {deletedShapes.Count} of {names.Count} shapes";
      if (failedShapes.Count > 0)
      {
         message += $" (failed: {string.Join(", ", failedShapes)})";
      }
      
      $"✅ {message}".WriteSuccess();
      
      if (deletedShapes.Count > 0)
      {
         ShapeChanged();
      }
      
      return new OPResult("DeleteMultipleShapes", ResultStatus.String, message);
   }

   public OPResult ClearShapes()
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return new OPResult("ClearShapes", ResultStatus.Error, "No stage connected");
      }

      var count = _stage.ClearAll();
      ShapeChanged();
      return new OPResult("ClearShapes", ResultStatus.String, $"Cleared {count} shapes from stage");
   }

   private OPResult FindShape(string shapeName)
   {
      if (_stage == null)
         return new OPResult("FindShape", ResultStatus.Error, "No stage connected");
      
      // Use Stage API - searches in FoGlyph3D slot where shapes are stored
      var (success, found) = _stage.FindMember<FoGlyph3D>(shapeName);
      var shape = found as FoShape3D;
      
      if (shape == null)
         return new OPResult("FindShape", ResultStatus.Error, $"Shape '{shapeName}' not found");
      
      return new OPResult("FindShape", ResultStatus.Shape3D, shape);
   }

   private void EnsureTransform(FoShape3D shape, string shapeName)
   {
      if (shape.Transform == null)
      {
         shape.Transform = new Transform3($"{shapeName}_Transform");
      }
   }
}
