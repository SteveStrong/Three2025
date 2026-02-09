#nullable enable
using FoundryMicroCore.Core.Extensions;

using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.PubSub;

namespace Three2025.Apprentice;

/// <summary>
/// Simple 2D shape editor following ModelEditor pattern.
/// Changes publish RefreshUIEvent for canvas updates.
/// </summary>
public class Shape2DEditor : IShape2DEditor
{
   private readonly IFoundryService _foundryService;
   private FoPage2D? _page;

   public Shape2DEditor(IFoundryService foundryService)
   {
      _foundryService = foundryService;
   }

   public void SetPage(FoPage2D page)
   {
      _page = page;
      $"🎨 Shape2DEditor connected to page: {page.GetName()}".WriteInfo();
   }
   
   private void ShapeChanged()
   {
      _foundryService.PubSub().Publish(RefreshUIEvent.TreeView());
   }

   // ============================================
   // PROPERTY OPERATIONS
   // ============================================

   public OPResult SetColor(string shapeName, string color)
   {
      var result = FindShape(shapeName);
      if (result.IsError()) 
         return result;

      var shape = result.AsShape2D();
      shape.Color = color;
      ShapeChanged();
      return result;
   }

   public OPResult SetPosition(string shapeName, double x, double y)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape2D();
      shape.MoveTo((int)x, (int)y);
      ShapeChanged();
      return result;
   }

   public OPResult MoveBy(string shapeName, double deltaX, double deltaY)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape2D();
      shape.MoveBy((int)deltaX, (int)deltaY);
      ShapeChanged();
      return result;
   }

   public OPResult SetDimensions(string shapeName, double width, double height)
   {
      var result = FindShape(shapeName);
      if (result.IsError())
         return result;

      var shape = result.AsShape2D();
      shape.Width = (int)width;
      shape.Height = (int)height;
      ShapeChanged();
      return result;
   }

   // ============================================
   // SHAPE CREATION
   // ============================================

   public OPResult AddRectangle(string name, double width, double height, string color, double x, double y)
   {
      if (_page == null)
         return new OPResult("AddRectangle", ResultStatus.Error, "No page connected");

      var shape = new FoShape2D((int)width, (int)height, color)
      {
         Name = name
      };
      
      shape.MoveTo((int)x, (int)y);
      _page.AddShape(shape);
      ShapeChanged();
      
      $"✅ Added rectangle '{name}' to page".WriteSuccess();
      return new OPResult("AddRectangle", ResultStatus.Shape2D, shape);
   }

   public OPResult AddCircle(string name, double radius, string color, double x, double y)
   {
      if (_page == null)
         return new OPResult("AddCircle", ResultStatus.Error, "No page connected");

      var diameter = radius * 2;
      var shape = new FoShape2D((int)diameter, (int)diameter, color)
      {
         Name = name
      };
      shape.OnDraw = shape.DrawCircle;
      
      shape.MoveTo((int)x, (int)y);
      _page.AddShape(shape);
      ShapeChanged();
      
      $"✅ Added circle '{name}' to page".WriteSuccess();
      return new OPResult("AddCircle", ResultStatus.Shape2D, shape);
   }

   public OPResult AddText(string name, string text, double x, double y, string color)
   {
      if (_page == null)
         return new OPResult("AddText", ResultStatus.Error, "No page connected");

      var shape = new FoText2D(name, 100, 30, color)
      {
         Text = text,
         TextColor = color
      };
      
      shape.MoveTo((int)x, (int)y);
      _page.AddShape(shape);
      ShapeChanged();
      
      $"✅ Added text '{name}' to page".WriteSuccess();
      return new OPResult("AddText", ResultStatus.Shape2D, shape);
   }

   // ============================================
   // CONNECTION OPERATIONS
   // ============================================

   public OPResult ConnectShapes(string connectorName, string startShapeName, string endShapeName, string color = "Black", double thickness = 2)
   {
      if (_page == null)
         return new OPResult("ConnectShapes", ResultStatus.Error, "No page connected");

      var startResult = FindShape(startShapeName);
      if (startResult.IsError())
         return new OPResult("ConnectShapes", ResultStatus.Error, $"Start shape not found: {startShapeName}");

      var endResult = FindShape(endShapeName);
      if (endResult.IsError())
         return new OPResult("ConnectShapes", ResultStatus.Error, $"End shape not found: {endShapeName}");

      var connector = new FoShape1D(connectorName, color)
      {
         Thickness = (float)thickness
      };

      var startShape = startResult.AsShape2D();
      var endShape = endResult.AsShape2D();
      
      connector.GlueStartTo(startShape);
      connector.GlueFinishTo(endShape);

      _page.AddShape(connector);
      ShapeChanged();

      $"✅ Connected {startShapeName} → {endShapeName}".WriteSuccess();
      return new OPResult("ConnectShapes", ResultStatus.Shape2D, connector);
   }

   // ============================================
   // HIERARCHY OPERATIONS
   // ============================================

   public OPResult GetChildShapes(string parentShapeName)
   {
      var result = FindShape(parentShapeName);
      if (result.IsError())
         return result;

      var parentShape = result.AsShape2D();
      // TODO: Replace with new collection API
      // var children = parentShape.GetMembers<FoGlyph2D>();
      var children = new List<FoGlyph2D>(); // Temporary placeholder
      
      // Return empty collection if no children (not an error - makes consumer code simpler)
      if (children == null || children.Count == 0)
      {
         return OPResult.Collection<FoGlyph2D>(new List<FoGlyph2D>());
      }
      
      // Return actual shape collection - consumer can query, filter, extract names, etc.
      return OPResult.Collection(children);
   }

   public OPResult RemoveChildShape(string parentShapeName, string childShapeName)
   {
      var result = FindShape(parentShapeName);
      if (result.IsError())
         return result;

      var parentShape = result.AsShape2D();
      
      // TODO: Replace with new collection API
      // var child = parentShape.GetMembers<FoGlyph2D>()?.FirstOrDefault(c => c.GetName() == childShapeName);
      var child = null as FoGlyph2D; // Temporary placeholder
      if (child == null)
      {
         return new OPResult("RemoveChildShape", ResultStatus.Error, $"Child shape '{childShapeName}' not found in parent '{parentShapeName}'");
      }
      
      parentShape.RemoveShape<FoGlyph2D>(child);
      ShapeChanged();
      
      return new OPResult("RemoveChildShape", ResultStatus.String, $"Removed child shape '{childShapeName}' from parent '{parentShapeName}'");
   }

   // ============================================
   // QUERY OPERATIONS
   // ============================================

   public OPResult GetShapeByName(string name)
   {
      return FindShape(name);
   }

   public OPResult GetAllShapes()
   {
      if (_page == null)
      {
         $"❌ No page connected".WriteError();
         return new OPResult("GetAllShapes", ResultStatus.Error, "No page connected");
      }

      var shapes = _page.AllShapes2D().Concat(_page.AllShapes1D()).ToList();
      $"📋 Retrieved {shapes.Count} shapes from page".WriteInfo();
      
      // Return as collection - consumer can filter, query, count, etc.
      return OPResult.Collection(shapes);
   }

   // ============================================
   // DELETION OPERATIONS
   // ============================================

   public OPResult DeleteShape(string name)
   {
      if (_page == null)
      {
         $"❌ No page connected".WriteError();
         return new OPResult("DeleteShape", ResultStatus.Error, "No page connected");
      }

      // TODO: Replace with new collection API
      // var shape = _page.GetMembers<FoGlyph2D>()?.FirstOrDefault(s => s.GetName() == name);
      var shape = null as FoGlyph2D; // Temporary placeholder

      if (shape != null)
      {
         shape.Delete();
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
      if (_page == null)
      {
         $"❌ No page connected".WriteError();
         return new OPResult("DeleteMultipleShapes", ResultStatus.Error, "No page connected");
      }

      var deletedShapes = new List<string>();
      var failedShapes = new List<string>();

      foreach (var name in names)
      {
         var shape = _page.FindShapes(name).FirstOrDefault();
         if (shape != null)
         {
            shape.Delete();
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
      if (_page == null)
      {
         $"❌ No page connected".WriteError();
         return new OPResult("ClearShapes", ResultStatus.Error, "No page connected");
      }

      var count = _page.ClearAll();
      ShapeChanged();
      return new OPResult("ClearShapes", ResultStatus.String, $"Cleared {count} shapes from page");
   }

   // ============================================
   // SHAPE MANAGEMENT
   // ============================================

   public OPResult AddShape(FoGlyph2D shape)
   {
      if (_page == null)
      {
         $"❌ No page connected".WriteError();
         return new OPResult("AddShape", ResultStatus.Error, "No page connected");
      }

      _page.AddShape(shape);
      ShapeChanged();
      $"✅ Added shape '{shape.GetName()}' to page".WriteSuccess();
      return new OPResult("AddShape", ResultStatus.Shape2D, shape);
   }

   // ============================================
   // HELPER METHODS
   // ============================================

   private OPResult FindShape(string shapeName)
   {
      if (_page == null)
         return new OPResult("FindShape", ResultStatus.Error, "No page connected");
      
      // Search in FoGlyph2D collection where shapes are stored
      var shape = _page.FindShapes(shapeName).FirstOrDefault();
      if (shape == null)
         return new OPResult("FindShape", ResultStatus.Error, $"Shape '{shapeName}' not found");
      
      return new OPResult("FindShape", ResultStatus.Shape2D, shape);
   }
}
