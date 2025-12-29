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
      shape.SetMaterialStale();  // Mark material for recompute
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

   public bool SetVisibility(string shapeName, bool isVisible)
   {
      var shape = FindShape(shapeName);
      if (shape == null)
      {
         $"❌ Shape '{shapeName}' not found".WriteError();
         return false;
      }

      ShapeChanged();
      return true;
   }

   private GeometryShape? FindShape(string shapeName)
   {
      if (_stage == null)
         return null;
      
      // Use Stage API - searches in FoGlyph3D slot where shapes are stored
      var (success, found) = _stage.FindMember<FoGlyph3D>(shapeName);
      return found as GeometryShape;
   }

   private void EnsureTransform(GeometryShape shape, string shapeName)
   {
      if (shape.Transform == null)
      {
         shape.Transform = new Transform3($"{shapeName}_Transform");
      }
   }
}
