#nullable enable
using FoundryMicroCore.Core.Extensions;

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
      if (shape == null || shape.Name == "Error")
         return OPResult.Error($"Could not retrieve shape '{shapeName}'");
         
      shape.Color = color;
      ShapeChanged();
      return OPResult.Success($"Changed color of '{shapeName}' to '{color}'");
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
      return OPResult.Success($"Moved '{shapeName}' to position ({position.X}, {position.Y}, {position.Z})");
   }

   public OPResult MoveBy(string shapeName, double deltaX, double deltaY, double deltaZ)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      EnsureTransform(shape, shapeName);
      shape.Transform!.MoveBy(deltaX, deltaY, deltaZ);
      ShapeChanged();
      return OPResult.Success($"Moved '{shapeName}' by ({deltaX}, {deltaY}, {deltaZ})");
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
      return OPResult.Success($"Set rotation of '{shapeName}' to ({rotation.X}, {rotation.Y}, {rotation.Z})");
   }

   public OPResult RotateBy(string shapeName, double xDegrees, double yDegrees, double zDegrees)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape3D();
      EnsureTransform(shape, shapeName);
      
      // Transform3.RotateBy expects radians and an angle unit
      shape.Transform!.RotateBy(xDegrees, yDegrees, zDegrees, AngleUnit.Degrees);
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
      
      parentShape.RemoveShape<FoGlyph3D>(child);
      ShapeChanged();
      
      return new OPResult("RemoveChildShape", ResultStatus.Shape3D, $"Removed child shape '{childShapeName}' from parent '{parentShapeName}'");
   }

   public OPResult GetChildShapes(string parentShapeName)
   {
      var result = FindShape(parentShapeName);
      if (result.IsError())
         return result;

      var parentShape = result.AsShape3D();
      // TODO: Replace with new collection API
      // var children = parentShape.GetMembers<FoGlyph3D>();
      var children = new List<FoGlyph3D>(); // Temporary placeholder
      
      // Return empty collection if no children (not an error - makes consumer code simpler)
      if (children == null || children.Count == 0)
      {
         return OPResult.Collection<FoGlyph3D>(new List<FoGlyph3D>());
      }
      
      // Return actual shape collection - consumer can query, filter, extract names, etc.
      return OPResult.Collection(children);
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

   public OPResult GetShapeByName(string name)
   {
      return FindShape(name);
   }

   public OPResult GetAllShapes()
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return new OPResult("GetAllShapes", ResultStatus.Error, "No stage connected");
      }

      $"🔍 GetAllShapes: Stage '{_stage.GetName()}' HashCode={_stage.GetHashCode()}".WriteInfo();
      
      // FIX: Use AllBodies() + AllLinks() instead of Members<FoGlyph3D>()
      // Members<FoGlyph3D>() only returns items from Slot<FoGlyph3D>(),
      // but AddShape stores in type-specific slots via DynamicSlot(type)
      var allBodies = _stage.AllBodies();
      var allLinks = _stage.AllLinks();
      var allGlyphs = allBodies.Concat(allLinks).ToList();
      
      $"🔍 GetAllShapes: AllBodies()={allBodies.Count()}, AllLinks()={allLinks.Count()}, Total={allGlyphs.Count} glyphs".WriteInfo();
      
      var shapes = allGlyphs.OfType<FoShape3D>().ToList();
      
      $"📋 Stage '{_stage.GetName()}' has {allGlyphs.Count} glyphs total, {shapes.Count} are FoShape3D".WriteInfo();
      
      // Debug: Show what's in the stage
      if (allGlyphs.Any())
      {
         "🔍 Glyphs in stage:".WriteInfo();
         foreach (var glyph in allGlyphs)
         {
            var glyphName = glyph.GetName() ?? "<null>";
            var glyphKey = glyph.Name ?? "<null>";
            var glyphUuid = glyph.GetGlyphId() ?? "<null>";
            var glyphType = glyph.GetType().Name;
            $"   - Type={glyphType}, Name='{glyphName}', Key='{glyphKey}', UUID={glyphUuid}".WriteInfo();
         }
      }
      
      // Return as collection - consumer can filter, query, count, etc.
      return OPResult.Collection(shapes);
   }

   public OPResult AddShape(FoShape3D shape)
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return new OPResult("AddShape", ResultStatus.Error, "No stage connected");
      }

      $"🔷 AddShape: Adding '{shape.GetName()}' (GlyphId={shape.GetGlyphId()}) to stage '{_stage.GetName()}' (HashCode={_stage.GetHashCode()})".WriteSuccess();
      _stage.AddShape(shape);
      
      // Verify it was added
      // TODO: Replace with new collection API
      // var verifyCount = _stage.Members<FoGlyph3D>().Count;
      var verifyCount = 0; // Temporary placeholder
      $"🔷 AddShape: Stage now has {verifyCount} glyphs after add".WriteSuccess();
      
      ShapeChanged();
      $"✅ Added shape '{shape.GetName()}' to stage".WriteSuccess();
      return new OPResult("AddShape", ResultStatus.Shape3D, shape);
   }

   private OPResult FindShape(string shapeName)
   {
      if (_stage == null)
         return new OPResult("FindShape", ResultStatus.Error, "No stage connected");
      
      $"🔍 FindShape: Searching for '{shapeName}' in stage '{_stage.GetName()}' HashCode={_stage.GetHashCode()}".WriteInfo();
      
      // Use Stage API - FindMember searches the Bodies/Links collections
      var (success, found) = _stage.FindMember<FoGlyph3D>(shapeName);
      var shape = found as FoShape3D;
      
      if (shape == null)
      {
         $"❌ FindShape: '{shapeName}' not found (success={success}, found={found?.GetType().Name ?? "null"})".WriteWarning();
         return new OPResult("FindShape", ResultStatus.Error, $"Shape '{shapeName}' not found");
      }
      
      $"✅ FindShape: Found '{shapeName}' → Name='{shape.GetName()}', Key='{shape.Name}'".WriteSuccess();
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
