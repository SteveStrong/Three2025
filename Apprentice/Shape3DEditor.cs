#nullable enable

using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler.Model;
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

   public bool SetColor(string shapeName, string color)
   {
      var shape = FindShape(shapeName);
      if (shape == null)
      {
         $"❌ Shape '{shapeName}' not found".WriteError();
         return false;
      }

      shape.Color = color;
      ShapeChanged();
      return true;
   }

   public bool SetPosition(string shapeName, Vector3 position)
   {
      var shape = FindShape(shapeName);
      if (shape == null)
      {
         $"❌ Shape '{shapeName}' not found".WriteError();
         return false;
      }

      EnsureTransform(shape, shapeName);
      shape.Transform!.Position = position;
      ShapeChanged();
      return true;
   }

   public bool SetRotation(string shapeName, Euler rotation)
   {
      var shape = FindShape(shapeName);
      if (shape == null)
      {
         $"❌ Shape '{shapeName}' not found".WriteError();
         return false;
      }

      EnsureTransform(shape, shapeName);
      shape.Transform!.Rotation = rotation;
      ShapeChanged();
      return true;
   }

   public bool SetScale(string shapeName, Vector3 scale)
   {
      var shape = FindShape(shapeName);
      if (shape == null)
      {
         $"❌ Shape '{shapeName}' not found".WriteError();
         return false;
      }

      EnsureTransform(shape, shapeName);
      shape.Transform!.Scale = scale;
      ShapeChanged();
      return true;
   }

   public bool SetDimensions(string shapeName, double width, double height, double depth)
   {
      var shape = FindShape(shapeName);
      if (shape == null)
      {
         $"❌ Shape '{shapeName}' not found".WriteError();
         return false;
      }

      shape.Width = width;
      shape.Height = height;
      shape.Depth = depth;
      ShapeChanged();
      return true;
   }

   public bool DeleteShape(string name)
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return false;
      }

      var (success, shape) = _stage.FindMember<FoGlyph3D>(name);
      var foShape = shape as FoShape3D;

      if (foShape != null)
      {
         foShape.DeleteFromStage(_stage);
         $"❌ Deleted shape '{name}'".WriteSuccess();
         ShapeChanged();
         return true;
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
         return false;
      }
   }

   public int DeleteMultipleShapes(List<string> names)
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return 0;
      }

      int deletedCount = 0;

      foreach (var name in names)
      {
         var (success, found) = _stage.FindMember<FoGlyph3D>(name);
         var shape = found as FoShape3D;
         if (shape != null)
         {
            shape.DeleteFromStage(_stage);
            deletedCount++;
         }
      }

      $"❌ Deleted {deletedCount} of {names.Count} shapes".WriteSuccess();
      
      if (deletedCount > 0)
      {
         ShapeChanged();
      }
      
      return deletedCount;
   }

   public bool ClearShapes()
   {
      if (_stage == null)
      {
         $"❌ No stage connected".WriteError();
         return false;
      }

      _ = _stage.ClearAll();
      ShapeChanged();
      return true;
   }

   private FoShape3D? FindShape(string shapeName)
   {
      if (_stage == null)
         return null;
      
      // Use Stage API - searches in FoGlyph3D slot where shapes are stored
      var (success, found) = _stage.FindMember<FoGlyph3D>(shapeName);
      return found as FoShape3D;
   }

   private void EnsureTransform(FoShape3D shape, string shapeName)
   {
      if (shape.Transform == null)
      {
         shape.Transform = new Transform3($"{shapeName}_Transform");
      }
   }
}
