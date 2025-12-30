#nullable enable

using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;

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



   public Shape3DTech(IWorkspace workspace, IFoundryService foundryService, IShape3DEditor shapeEditor)
   {
      Workspace = workspace;
      FoundryService = foundryService;
      ShapeEditor = shapeEditor;
   }

   /// <summary>
   /// Set the stage to use. Call this from a page to inject its stage.
   /// </summary>
   public void SetStage(FoStage3D stage)
   {
      Stage = stage;
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
   public FoStage3D EstablishGeometryStage(string? stageName = null)
   {

      if (Stage != null)
      {
         // Ensure ShapeEditor always has the stage, even on repeated calls
         ShapeEditor.SetStage(Stage);
         return Stage;
      }

      var arena = Workspace.GetArena();

      // If stage name is specified, use it; otherwise use "AgentCanvas3D" as default
      var stageToUse = stageName ?? "AgentCanvas3D";
      Stage = arena.EstablishStage<FoStage3D>(stageToUse);

      // Connect ShapeEditor to this stage
      ShapeEditor.SetStage(Stage);

      $"Shape3DTech: Connected to stage '{stageToUse}'".WriteInfo();




      RefreshUI();
      return Stage;
   }

   [Description("Clears all shapes from the geometry stage")]
   public void ClearShapes()
   {
      ShapeEditor.ClearShapes();
   }

   [Description("Saves all shapes to a file for persistence")]
   public void SaveShapes()
   {
      EstablishGeometryStage();
      var result = ShapeEditor.GetAllShapes();
      if (result.IsError())
      {
         $"❌ Failed to get shapes: {result.Display()}".WriteError();
         return;
      }
      // Use Value() method and cast to List<FoShape3D>
      var shapeList = result.Value() as List<FoShape3D> ?? new List<FoShape3D>();
      var shapes = shapeList.OfType<GeometryShape>().ToList();
      var data = CodingExtensions.DehydrateList<GeometryShape>(shapes, false);
      FileHelpers.WriteData("Data", "shapes.json", data);
   }

   [Description("Restores shapes from a saved file")]
   public void RestoreShapes()
   {
      var data = FileHelpers.ReadData("Data", "shapes.json");
      var list = CodingExtensions.HydrateList<GeometryShape>(data, false);

      EstablishGeometryStage();
      ShapeEditor.ClearShapes();

      foreach (var item in list)
      {
         ShapeEditor.AddShape(item);
      }
   }

   [Description("Generate a Random Color")]
   public string PickARandomColor()
   {
      var color = DataGenerator.GenerateColor();
      return color;
   }

   [Description("Gets comprehensive information about all available tool capabilities, operations, and supported shape types")]
   public ToolCapabilities GetToolCapabilities()
   {
      return new ToolCapabilities
      {
         ToolName = "Shape3DTech",
         Version = "1.0",
         Description = "Comprehensive 3D shape manipulation tool for creating, transforming, querying, and managing geometric objects in a 3D scene.",
         CoordinateSystem = "Right-handed: X (left-/right+), Y (down-/up+), Z (back-/forward+). Origin at (0,0,0).",
         SupportedShapeTypes = new List<string>
         {
            "box", "sphere", "cylinder", "cone", "torus",
            "tetrahedron", "octahedron", "dodecahedron", "icosahedron",
            "torusknot", "capsule", "plane", "circle", "ring"
         },
         Categories = new List<CapabilityCategory>
         {
            new CapabilityCategory
            {
               CategoryName = "Shape Creation",
               Description = "Create new geometric shapes with various properties",
               Operations = new List<ToolOperation>
               {
                  new ToolOperation
                  {
                     MethodName = "AddShape",
                     Description = "Create a new shape with default dimensions",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Unique name for the shape" },
                         new() { Name = "color", Type = "string", Description = "Color name or hex code (#RRGGBB)" },
                        new() { Name = "shapeType", Type = "string", Description = "Type of shape to create", DefaultValue = "box" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "AddShape('RedBox', true, 'red', 'box')"
                  },
                  new ToolOperation
                  {
                     MethodName = "AddShapeWithDimensions",
                     Description = "Create a new shape with specific dimensions",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Unique name" },
                         new() { Name = "color", Type = "string", Description = "Color" },
                        new() { Name = "shapeType", Type = "string", Description = "Shape type" },
                        new() { Name = "width", Type = "double", Description = "Width (X dimension)" },
                        new() { Name = "height", Type = "double", Description = "Height (Y dimension)" },
                        new() { Name = "depth", Type = "double", Description = "Depth (Z dimension)" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "AddShapeWithDimensions('CustomBox', true, 'blue', 'box', 3.0, 2.0, 1.5)"
                  },
                  new ToolOperation
                  {
                     MethodName = "DuplicateShape",
                     Description = "Copy an existing shape with position offset",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "sourceName", Type = "string", Description = "Name of shape to duplicate" },
                        new() { Name = "newName", Type = "string", Description = "Name for the copy" },
                        new() { Name = "offsetX", Type = "double", Description = "X offset from original" },
                        new() { Name = "offsetY", Type = "double", Description = "Y offset from original" },
                        new() { Name = "offsetZ", Type = "double", Description = "Z offset from original" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "DuplicateShape('Box1', 'Box2', 5.0, 0, 0)"
                  }
               }
            },
            new CapabilityCategory
            {
               CategoryName = "Transformations",
               Description = "Move, rotate, scale, and resize shapes",
               Operations = new List<ToolOperation>
               {
                  new ToolOperation
                  {
                     MethodName = "RepositionShape",
                     Description = "Move shape to absolute position",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" },
                        new() { Name = "x", Type = "double", Description = "X coordinate" },
                        new() { Name = "y", Type = "double", Description = "Y coordinate" },
                        new() { Name = "z", Type = "double", Description = "Z coordinate" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "RepositionShape('Box1', 5.0, 2.0, -3.0)"
                  },
                  new ToolOperation
                  {
                     MethodName = "RotateShape",
                     Description = "Rotate shape around X, Y, Z axes (degrees)",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" },
                        new() { Name = "xDegrees", Type = "double", Description = "X rotation in degrees" },
                        new() { Name = "yDegrees", Type = "double", Description = "Y rotation in degrees" },
                        new() { Name = "zDegrees", Type = "double", Description = "Z rotation in degrees" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "RotateShape('Box1', 45, 0, 90)"
                  },
                  new ToolOperation
                  {
                     MethodName = "ScaleShape",
                     Description = "Scale shape by multipliers (1.0 = original size)",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" },
                        new() { Name = "scaleX", Type = "double", Description = "X scale factor" },
                        new() { Name = "scaleY", Type = "double", Description = "Y scale factor" },
                        new() { Name = "scaleZ", Type = "double", Description = "Z scale factor" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "ScaleShape('Box1', 2.0, 1.5, 1.0)"
                  },
                  new ToolOperation
                  {
                     MethodName = "ChangeShapeDimensions",
                     Description = "Change width, height, depth of existing shape",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" },
                        new() { Name = "width", Type = "double", Description = "New width" },
                        new() { Name = "height", Type = "double", Description = "New height" },
                        new() { Name = "depth", Type = "double", Description = "New depth" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "ChangeShapeDimensions('Box1', 3.0, 4.0, 2.0)"
                  }
               }
            },
            new CapabilityCategory
            {
               CategoryName = "Appearance",
               Description = "Modify visual properties of shapes",
               Operations = new List<ToolOperation>
               {
                  new ToolOperation
                  {
                     MethodName = "ChangeColor",
                     Description = "Change shape color",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" },
                        new() { Name = "color", Type = "string", Description = "New color (name or hex)" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "ChangeColor('Box1', '#ff0000')"
                  },
                  new ToolOperation
                  {
                     MethodName = "ChangeState",
                     Description = "Show or hide shape",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" },
                      },
                     ReturnType = "List<ShapeInfo>",
                     Example = "ChangeState('Box1', false)"
                  },
                  new ToolOperation
                  {
                     MethodName = "PickARandomColor",
                     Description = "Get a random color name",
                     Parameters = new List<OperationParameter>(),
                     ReturnType = "string",
                     Example = "PickARandomColor()"
                  }
               }
            },
            new CapabilityCategory
            {
               CategoryName = "Querying",
               Description = "Get information about shapes in the scene",
               Operations = new List<ToolOperation>
               {
                  new ToolOperation
                  {
                     MethodName = "GetShapes",
                     Description = "List all shapes with complete information",
                     Parameters = new List<OperationParameter>(),
                     ReturnType = "List<ShapeInfo>",
                     Example = "GetShapes()"
                  },
                  new ToolOperation
                  {
                     MethodName = "GetShapeByName",
                     Description = "Get detailed info about a specific shape",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" }
                     },
                     ReturnType = "ShapeInfo",
                     Example = "GetShapeByName('Box1')"
                  },
                  new ToolOperation
                  {
                     MethodName = "GetToolCapabilities",
                     Description = "Get this capability information",
                     Parameters = new List<OperationParameter>(),
                     ReturnType = "ToolCapabilities",
                     Example = "GetToolCapabilities()"
                  }
               }
            },
            new CapabilityCategory
            {
               CategoryName = "Deletion",
               Description = "Remove shapes from the scene",
               Operations = new List<ToolOperation>
               {
                  new ToolOperation
                  {
                     MethodName = "DeleteShape",
                     Description = "Delete a single shape",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "name", Type = "string", Description = "Shape name" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "DeleteShape('Box1')"
                  },
                  new ToolOperation
                  {
                     MethodName = "DeleteMultipleShapes",
                     Description = "Delete multiple shapes at once",
                     Parameters = new List<OperationParameter>
                     {
                        new() { Name = "names", Type = "List<string>", Description = "List of shape names" }
                     },
                     ReturnType = "List<ShapeInfo>",
                     Example = "DeleteMultipleShapes(['Box1', 'Box2', 'Sphere1'])"
                  },
                  new ToolOperation
                  {
                     MethodName = "ClearShapes",
                     Description = "Remove all shapes from scene",
                     Parameters = new List<OperationParameter>(),
                     ReturnType = "void",
                     Example = "ClearShapes()"
                  }
               }
            },
            new CapabilityCategory
            {
               CategoryName = "Persistence",
               Description = "Save and restore scenes",
               Operations = new List<ToolOperation>
               {
                  new ToolOperation
                  {
                     MethodName = "SaveShapes",
                     Description = "Save current scene to file",
                     Parameters = new List<OperationParameter>(),
                     ReturnType = "void",
                     Example = "SaveShapes()"
                  },
                  new ToolOperation
                  {
                     MethodName = "RestoreShapes",
                     Description = "Load scene from file",
                     Parameters = new List<OperationParameter>(),
                     ReturnType = "void",
                     Example = "RestoreShapes()"
                  }
               }
            }
         }
      };
   }

   [Description("Gets a list of all shapes and their current state")]
   public List<FoShape3D> GetShapes()
   {
      EstablishGeometryStage();
      var result = ShapeEditor.GetAllShapes();
      if (result.IsError())
      {
         $"❌ Failed to get shapes: {result.Display()}".WriteError();
         return new List<FoShape3D>();
      }
      
      // Use Value() method and cast to List<FoShape3D>
      if (result.Value() is List<FoShape3D> shapeList)
         return shapeList;
         
      return new List<FoShape3D>();
   }

   [Description("Gets information about a specific shape by name")]
   public FoShape3D? GetShapeByName(
      [Description("The name of the shape to query")] string name)
   {
      EstablishGeometryStage();
      var result = ShapeEditor.GetShapeByName(name);
      if (result.IsError())
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
         return null;
      }
      $"🔍 Found shape '{name}'".WriteInfo();
      return result.AsShape3D();
   }

   [Description("Create and add a 3D shape to the geometry stage")]
   public List<FoShape3D> AddShape(
      [Description("The name of the shape to create")] string name,
      [Description("The color of the shape")] string color,
      [Description("The type of shape: box, sphere, cylinder, cone, torus, tetrahedron, octahedron, dodecahedron, icosahedron, torusknot, capsule, plane, circle, ring")] string shapeType = "box",
      [Description("X coordinate position (optional, defaults to 0)")] double x = 0.0,
      [Description("Y coordinate position (optional, defaults to 0)")] double y = 0.0,
      [Description("Z coordinate position (optional, defaults to 0)")] double z = 0.0)
   {
      EstablishGeometryStage();

      var newShape = new GeometryShape(name, shapeType)
      {
         Color = color
      };

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

      return GetShapes();
   }

   [Description("Create and add a 3D shape with specific dimensions")]
   public List<FoShape3D> AddShapeWithDimensions(
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
      EstablishGeometryStage();

      var newShape = new GeometryShape(name, shapeType, width, height, depth)
      {
         Color = color,
         Transform = new Transform3($"{name}_Transform")
         { 
            Position = new Vector3(x, y, z)
         }
      };

      ShapeEditor.AddShape(newShape);

      $"✅ Created {shapeType} '{name}' ({width}x{height}x{depth}) with color '{color}' at ({x:F1}, {y:F1}, {z:F1})".WriteSuccess();

      return GetShapes();
   }


   [Description("Delete a shape from the geometry stage")]
   public List<FoShape3D> DeleteShape(
      [Description("The name of the shape to delete")] string name)
   {
      ShapeEditor.DeleteShape(name);
      return GetShapes();
   }

   [Description("Delete multiple shapes from the geometry stage")]
   public List<FoShape3D> DeleteMultipleShapes(
      [Description("List of shape names to delete")] List<string> names)
   {
      ShapeEditor.DeleteMultipleShapes(names);
      return GetShapes();
   }

   [Description("Changes the X, Y, Z position of a shape in 3D space")]
   public List<FoShape3D> RepositionShape(
      [Description("The name of the shape to reposition")] string name,
      [Description("The X coordinate")] double x,
      [Description("The Y coordinate")] double y,
      [Description("The Z coordinate")] double z)
   {
      EstablishGeometryStage();

      // Use ShapeEditor for event-driven update
      var result = ShapeEditor.SetPosition(name, new Vector3(x, y, z));

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"✅ {result.Display()}".WriteSuccess();
      }

      return GetShapes();
   }

   [Description("Rotates a shape around X, Y, Z axes in degrees")]
   public List<FoShape3D> RotateShape(
      [Description("The name of the shape to rotate")] string name,
      [Description("Rotation around X axis in degrees")] double xDegrees,
      [Description("Rotation around Y axis in degrees")] double yDegrees,
      [Description("Rotation around Z axis in degrees")] double zDegrees)
   {
      EstablishGeometryStage();

      // Convert degrees to radians
      var xRad = xDegrees * Math.PI / 180.0;
      var yRad = yDegrees * Math.PI / 180.0;
      var zRad = zDegrees * Math.PI / 180.0;

      // Use ShapeEditor for event-driven update
      var result = ShapeEditor.SetRotation(name, new Euler(xRad, yRad, zRad));

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"✅ {result.Display()}".WriteSuccess();
      }

      return GetShapes();
   }

   [Description("Scales a shape by multiplying its size on X, Y, Z axes")]
   public List<FoShape3D> ScaleShape(
      [Description("The name of the shape to scale")] string name,
      [Description("Scale factor for X axis (1.0 = original size)")] double scaleX,
      [Description("Scale factor for Y axis (1.0 = original size)")] double scaleY,
      [Description("Scale factor for Z axis (1.0 = original size)")] double scaleZ)
   {
      EstablishGeometryStage();

      // Use ShapeEditor for event-driven update
      var result = ShapeEditor.SetScale(name, new Vector3(scaleX, scaleY, scaleZ));

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"✅ {result.Display()}".WriteSuccess();
      }

      return GetShapes();
   }

   [Description("Changes the dimensions (width, height, depth) of an existing shape")]
   public List<FoShape3D> ChangeShapeDimensions(
      [Description("The name of the shape")] string name,
      [Description("New width (X dimension)")] double width,
      [Description("New height (Y dimension)")] double height,
      [Description("New depth (Z dimension)")] double depth)
   {
      EstablishGeometryStage();

      // Use ShapeEditor for event-driven update
      var result = ShapeEditor.SetDimensions(name, width, height, depth);

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"✅ {result.Display()}".WriteSuccess();
      }

      return GetShapes();
   }



   [Description("Changes the color of a shape")]
   public List<FoShape3D> ChangeColor(
      [Description("The name of the shape")] string name,
      [Description("The new color for the shape")] string color)
   {
      $"🔧 ChangeColor CALLED: name='{name}', color='{color}'".WriteInfo();

      EstablishGeometryStage();
      $"📦 Stage established".WriteInfo();

      // Use ShapeEditor for event-driven update
      var result = ShapeEditor.SetColor(name, color);

      if (result.IsError())
      {
         $"❌ {result.Display()}".WriteError();
      }
      else
      {
         $"✅ {result.Display()}".WriteSuccess();
      }

      var shapes = GetShapes();
      $"📤 Returning {shapes.Count} shapes".WriteInfo();

      return shapes;
   }

   [Description("Changes the geometry type of an existing shape")]
   public List<FoShape3D> ChangeGeometry(
      [Description("The name of the shape")] string name,
      [Description("The new geometry type: box, sphere, cylinder, cone, torus, tetrahedron, octahedron, dodecahedron, icosahedron, torusknot, capsule, plane, circle, ring")] string shapeType,
      [Description("Optional new width (X dimension)")] double? width = null,
      [Description("Optional new height (Y dimension)")] double? height = null,
      [Description("Optional new depth (Z dimension)")] double? depth = null)
   {
      EstablishGeometryStage();

      // Use ShapeEditor for event-driven update
      var result = ShapeEditor.SetGeometry(name, shapeType, width, height, depth);

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"✅ Changed geometry of '{name}' to '{shapeType}'".WriteSuccess();
      }

      return GetShapes();
   }

   [Description("Establish a text label on a shape (creates if missing, updates if exists)")]
   public List<FoShape3D> EstablishTextLabel(
      [Description("The name of the parent shape")] string parentShapeName,
      [Description("The name for the label")] string labelName,
      [Description("The text to display")] string text,
      [Description("X position relative to parent (optional)")] double? relativeX = null,
      [Description("Y position relative to parent (optional, defaults to 2)")] double? relativeY = null,
      [Description("Z position relative to parent (optional)")] double? relativeZ = null,
      [Description("Font size (optional, defaults to 0.5)")] double? fontSize = null,
      [Description("Text color (optional, defaults to 'black')")] string? color = null)
   {
      EstablishGeometryStage();

      Vector3? position = null;
      if (relativeX.HasValue || relativeY.HasValue || relativeZ.HasValue)
      {
         position = new Vector3(relativeX ?? 0, relativeY ?? 2, relativeZ ?? 0);
      }

      var result = ShapeEditor.EstablishTextLabel(parentShapeName, labelName, text, position, fontSize, color);

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         var label = result.Value() as FoText3D;
         $"✅ Established text label '{labelName}' on '{parentShapeName}': '{label?.Text}'".WriteSuccess();
      }

      return GetShapes();
   }

   [Description("Remove a child shape from its parent shape")]
   public List<FoShape3D> RemoveChildShape(
      [Description("The name of the parent shape")] string parentShapeName,
      [Description("The name of the child shape to remove")] string childShapeName)
   {
      EstablishGeometryStage();

      var result = ShapeEditor.RemoveChildShape(parentShapeName, childShapeName);

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"✅ {result.Display()}".WriteSuccess();
      }

      return GetShapes();
   }

   [Description("Get the list of child shapes for a parent shape")]
   public string GetChildShapes(
      [Description("The name of the parent shape")] string parentShapeName)
   {
      EstablishGeometryStage();

      var result = ShapeEditor.GetChildShapes(parentShapeName);

      if (result.IsError())
      {
         $"⚠️  {result.Display()}".WriteWarning();
      }
      else
      {
         $"📋 {result.Display()}".WriteInfo();
      }

      return result.Display();
   }

   [Description("Create a link shape (IBodyLink3D) with dynamic geometry type support (Pipe/Tube/Line)")]
   public LinkShape CreateLinkShape(
      [Description("The name for the link")] string name,
      [Description("The color of the link")] string color,
      [Description("The starting body shape")] FoShape3D fromShape,
      [Description("The ending body shape")] FoShape3D toShape,
      [Description("The radius of the link")] double radius,
      [Description("Geometry type: Pipe, Tube, or Line")] string geomType = "Pipe")
   {
      EstablishGeometryStage();

      var link = new LinkShape(name, color, geomType)
      {
         FromShape3D = fromShape,
         ToShape3D = toShape,
         Radius = radius
      };

      ShapeEditor.AddShape(link);

      $"✅ Created link '{name}' [{geomType}] connecting '{fromShape.GetName()}' to '{toShape.GetName()}'".WriteSuccess();
      $"   📍 Link is IBodyLink3D (dependent shape) - renders AFTER bodies".WriteInfo();

      return link;
   }

   [Description("Change the geometry type of a link shape (Pipe/Tube/Line)")]
   public LinkShape ChangeLinkGeometryType(
      [Description("The name of the link to modify")] string name,
      [Description("New geometry type: Pipe, Tube, or Line")] string geomType)
   {
      EstablishGeometryStage();
      var result = ShapeEditor.GetShapeByName(name);

      if (result.IsError())
         throw new InvalidOperationException($"Link shape '{name}' not found");

      var link = result.AsShape3D() as LinkShape;
      if (link == null)
         throw new InvalidOperationException($"Shape '{name}' is not a LinkShape");

      link.SetLinkGeometry(geomType);

      $"✅ Changed '{name}' geometry to {geomType}".WriteSuccess();

      return link;
   }

   [Description("Create a pipe link shape (IBodyLink3D) connecting two body shapes")]
   public FoPipe3D CreatePipeLink(
      [Description("The name for the pipe link")] string name,
      [Description("The color of the pipe")] string color,
      [Description("The starting body shape")] FoShape3D fromShape,
      [Description("The ending body shape")] FoShape3D toShape,
      [Description("The radius of the pipe")] double radius)
   {
      EstablishGeometryStage();

      var pipe = new FoPipe3D(name, color)
      {
         FromShape3D = fromShape,
         ToShape3D = toShape
      };
      pipe.CreatePipe(name, radius);

      ShapeEditor.AddShape(pipe);

      $"✅ Created pipe link '{name}' connecting '{fromShape.GetName()}' to '{toShape.GetName()}'" .WriteSuccess();
      $"   📍 Pipe is IBodyLink3D (dependent shape) - renders AFTER bodies".WriteInfo();

      return pipe;
   }

   [Description("Create a pathway link shape (IBodyLink3D) connecting two body shapes")]
   public FoPathway3D CreatePathwayLink(
      [Description("The name for the pathway link")] string name,
      [Description("The starting body shape")] FoShape3D fromShape,
      [Description("The ending body shape")] FoShape3D toShape)
   {
      EstablishGeometryStage();

      var pathway = new FoPathway3D(name)
      {
         FromShape3D = fromShape,
         ToShape3D = toShape
      };

      ShapeEditor.AddShape(pathway);

      $"✅ Created pathway link '{name}' connecting '{fromShape.GetName()}' to '{toShape.GetName()}'".WriteSuccess();
      $"   📍 Pathway is IBodyLink3D (dependent shape) - renders AFTER bodies".WriteInfo();

      return pathway;
   }


}
