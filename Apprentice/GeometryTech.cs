using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

using FoundryWorldsAndDrawings.ThreeD.Maths;


namespace Three2025.Apprentice;
#nullable enable


public interface IGeometryTech : ITechnician
{
   void SetStage(FoStage3D stage);

   FoStage3D EstablishGeometryStage();

   ToolCapabilities GetToolCapabilities();

   void ClearShapes();

   void SaveShapes();

   void RestoreShapes();

   string PickARandomColor();

   List<ShapeInfo> GetShapes();

   ShapeInfo? GetShapeByName(string name);

   List<ShapeInfo> AddShape(string name, bool isOn, string color, string shapeType);

   List<ShapeInfo> AddShapeWithDimensions(string name, bool isOn, string color, string shapeType, double width, double height, double depth);

   List<ShapeInfo> DeleteShape(string name);

   List<ShapeInfo> DeleteMultipleShapes(List<string> names);

   List<ShapeInfo> RepositionShape(string name, double x, double y, double z);

   List<ShapeInfo> RotateShape(string name, double xDegrees, double yDegrees, double zDegrees);

   List<ShapeInfo> ScaleShape(string name, double scaleX, double scaleY, double scaleZ);

   List<ShapeInfo> ChangeShapeDimensions(string name, double width, double height, double depth);

   List<ShapeInfo> ChangeState(string name, bool isOn);

   List<ShapeInfo> ChangeColor(string name, string color);

   List<ShapeInfo> DuplicateShape(string sourceName, string newName, double offsetX, double offsetY, double offsetZ);
}

public class ToolCapabilities
{
   public string ToolName { get; set; } = "GeometryTech";
   public string Version { get; set; } = "1.0";
   public string Description { get; set; } = "";
   public List<CapabilityCategory> Categories { get; set; } = new();
   public List<string> SupportedShapeTypes { get; set; } = new();
   public string CoordinateSystem { get; set; } = "";
}

public class CapabilityCategory
{
   public string CategoryName { get; set; } = "";
   public string Description { get; set; } = "";
   public List<ToolOperation> Operations { get; set; } = new();
}

public class ToolOperation
{
   public string MethodName { get; set; } = "";
   public string Description { get; set; } = "";
   public List<OperationParameter> Parameters { get; set; } = new();
   public string ReturnType { get; set; } = "";
   public string Example { get; set; } = "";
}

public class OperationParameter
{
   public string Name { get; set; } = "";
   public string Type { get; set; } = "";
   public string Description { get; set; } = "";
   public string? DefaultValue { get; set; }
}

public class ShapeInfo
{
   public string Name { get; set; } = "";
   public string GeomType { get; set; } = "";
   public string Color { get; set; } = "";
   public bool IsVisible { get; set; }
   public double X { get; set; }
   public double Y { get; set; }
   public double Z { get; set; }
   public double Width { get; set; }
   public double Height { get; set; }
   public double Depth { get; set; }
   public double RotationX { get; set; }
   public double RotationY { get; set; }
   public double RotationZ { get; set; }
   public double ScaleX { get; set; }
   public double ScaleY { get; set; }
   public double ScaleZ { get; set; }
}

public class GeometryTech : IGeometryTech
{

   private IWorkspace Workspace;
   private IFoundryService FoundryService;

   private FoStage3D? Stage { get; set; }


   private MockDataGenerator DataGenerator { get; set; } = new();



   public GeometryTech(IWorkspace workspace, IFoundryService foundryService)
   {
      Workspace = workspace;
      FoundryService = foundryService;
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
         FoundryService.PubSub().Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());
         "🔄 UI Refresh triggered".WriteInfo();
      }
      catch (Exception ex)
      {
         $"⚠️ RefreshUI failed: {ex.Message}".WriteWarning();
      }
   }
   
   [Description("Establish a Geometry Stage for managing 3D shapes in the application")]
   public FoStage3D EstablishGeometryStage()
   {

      if ( Stage != null)
         return Stage;

      var arena = Workspace.GetArena();
      Stage = arena.EstablishStage<FoStage3D>("Geometry");



      // var shapes = new List<GeometryShape>()
      // {
      //    new GeometryShape("Red Box") { IsOn = false, Color = DataGenerator.GenerateColor() },
      //    new GeometryShape("Blue Sphere") { IsOn = false, Color = DataGenerator.GenerateColor() },
      //    new GeometryShape("Green Cylinder") { IsOn = true, Color = DataGenerator.GenerateColor() }
      // };

      // foreach (var shape in shapes)
      // {
      //    arena.AddShapeToStage<GeometryShape>(shape);
      // }


      RefreshUI();
      return Stage;
   }

   [Description("Clears all shapes from the geometry stage")]
   public void ClearShapes()
   {
      var stage = EstablishGeometryStage();
      _ = stage.ClearAll();
      RefreshUI();
   }

   [Description("Saves all shapes to a file for persistence")]
   public void SaveShapes()
   {  
      var stage = EstablishGeometryStage();
      var shapes = stage.Members<GeometryShape>();
      var data = CodingExtensions.DehydrateList<GeometryShape>(shapes,false);
      FileHelpers.WriteData("Data", "shapes.json", data);
   }

   [Description("Restores shapes from a saved file")]
   public void RestoreShapes()
   {
      var data = FileHelpers.ReadData("Data", "shapes.json");
      var list = CodingExtensions.HydrateList<GeometryShape>(data,false);
      
   
      var stage = EstablishGeometryStage();
      _ = stage.ClearAll();

      foreach (var item in list)
      {
         stage.AddShape(item);
      }
      RefreshUI();

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
         ToolName = "GeometryTech",
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
                        new() { Name = "isOn", Type = "bool", Description = "Visibility (true=visible, false=hidden)" },
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
                        new() { Name = "isOn", Type = "bool", Description = "Visibility" },
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
                        new() { Name = "isOn", Type = "bool", Description = "true=show, false=hide" }
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
   public List<ShapeInfo> GetShapes()
   {
      var stage = EstablishGeometryStage();
      var shapes = stage.Members<GeometryShape>();
      $"📋 Retrieved {shapes.Count} shapes from stage".WriteInfo();
      return shapes.Select(s => ConvertToShapeInfo(s)).ToList();
   }

   [Description("Gets information about a specific shape by name")]
   public ShapeInfo? GetShapeByName(
      [Description("The name of the shape to query")] string name)
   {
      var list = GetShapes();
      var shape = list.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
      
      if (shape != null)
      {
         $"🔍 Found shape '{name}'".WriteInfo();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }
      
      return shape;
   }

   private ShapeInfo ConvertToShapeInfo(GeometryShape shape)
   {
      var pos = shape.Transform?.Position ?? Vector3.Zero;
      var rot = shape.Transform?.Rotation ?? new Euler(0, 0, 0);
      var scale = shape.Transform?.Scale ?? new Vector3(1, 1, 1);
      
      return new ShapeInfo
      {
         Name = shape.GetName(),
         GeomType = shape.GeomType,
         Color = shape.Color,
         IsVisible = shape.IsOn ?? false,
         X = pos.X,
         Y = pos.Y,
         Z = pos.Z,
         Width = shape.Width,
         Height = shape.Height,
         Depth = shape.Depth,
         RotationX = rot.X,
         RotationY = rot.Y,
         RotationZ = rot.Z,
         ScaleX = scale.X,
         ScaleY = scale.Y,
         ScaleZ = scale.Z
      };
   }
   
   [Description("Create and add a 3D shape to the geometry stage")]
   public List<ShapeInfo> AddShape(
      [Description("The name of the shape to create")] string name, 
      [Description("Whether the shape should be visible/active")] bool isOn, 
      [Description("The color of the shape")] string color,
      [Description("The type of shape: box, sphere, cylinder, cone, torus, tetrahedron, octahedron, dodecahedron, icosahedron, torusknot, capsule, plane, circle, ring")] string shapeType = "box")
   {
      var stage = EstablishGeometryStage();

      var newShape = new GeometryShape(name, shapeType)
      {
         IsOn = isOn,
         Color = color
      };

      stage.AddShape(newShape);

      $"✅ Created {shapeType} '{name}' with color '{color}', visible={isOn}".WriteSuccess();

      RefreshUI();

      return GetShapes();
   }

   [Description("Create and add a 3D shape with specific dimensions")]
   public List<ShapeInfo> AddShapeWithDimensions(
      [Description("The name of the shape to create")] string name,
      [Description("Whether the shape should be visible/active")] bool isOn,
      [Description("The color of the shape")] string color,
      [Description("The type of shape")] string shapeType,
      [Description("Width (X dimension)")] double width,
      [Description("Height (Y dimension)")] double height,
      [Description("Depth (Z dimension)")] double depth)
   {
      var stage = EstablishGeometryStage();

      var newShape = new GeometryShape(name, shapeType, width, height, depth)
      {
         IsOn = isOn,
         Color = color
      };

      stage.AddShape(newShape);

      $"✅ Created {shapeType} '{name}' ({width}x{height}x{depth}) with color '{color}'".WriteSuccess();

      RefreshUI();

      return GetShapes();
   }


   [Description("Delete a shape from the geometry stage")]
   public List<ShapeInfo> DeleteShape(
      [Description("The name of the shape to delete")] string name)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(shape => shape.GetName().Matches(name));

      if (shape != null)
      {
         shape.DeleteFromStage(stage);
         $"❌ Deleted shape '{name}'".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();

      return GetShapes();
   }

   [Description("Delete multiple shapes from the geometry stage")]
   public List<ShapeInfo> DeleteMultipleShapes(
      [Description("List of shape names to delete")] List<string> names)
   {
      var stage = EstablishGeometryStage();
      int deletedCount = 0;

      foreach (var name in names)
      {
         var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));
         if (shape != null)
         {
            shape.DeleteFromStage(stage);
            deletedCount++;
         }
      }

      $"❌ Deleted {deletedCount} of {names.Count} shapes".WriteSuccess();

      RefreshUI();

      return GetShapes();
   }

   [Description("Changes the X, Y, Z position of a shape in 3D space")]
   public List<ShapeInfo> RepositionShape(
      [Description("The name of the shape to reposition")] string name, 
      [Description("The X coordinate")] double x, 
      [Description("The Y coordinate")] double y, 
      [Description("The Z coordinate")] double z)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));

      if (shape != null)
      {
         shape.Transform!.Position = new Vector3(x, y, z);
         $"📍 Shape '{name}' repositioned to ({x:F1}, {y:F1}, {z:F1})".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }

   [Description("Rotates a shape around X, Y, Z axes in degrees")]
   public List<ShapeInfo> RotateShape(
      [Description("The name of the shape to rotate")] string name,
      [Description("Rotation around X axis in degrees")] double xDegrees,
      [Description("Rotation around Y axis in degrees")] double yDegrees,
      [Description("Rotation around Z axis in degrees")] double zDegrees)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));

      if (shape != null)
      {
         // Convert degrees to radians
         var xRad = xDegrees * Math.PI / 180.0;
         var yRad = yDegrees * Math.PI / 180.0;
         var zRad = zDegrees * Math.PI / 180.0;
         
         shape.Transform!.Rotation = new Euler(xRad, yRad, zRad);
         $"🔄 Shape '{name}' rotated to ({xDegrees:F1}°, {yDegrees:F1}°, {zDegrees:F1}°)".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }

   [Description("Scales a shape by multiplying its size on X, Y, Z axes")]
   public List<ShapeInfo> ScaleShape(
      [Description("The name of the shape to scale")] string name,
      [Description("Scale factor for X axis (1.0 = original size)")] double scaleX,
      [Description("Scale factor for Y axis (1.0 = original size)")] double scaleY,
      [Description("Scale factor for Z axis (1.0 = original size)")] double scaleZ)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));

      if (shape != null)
      {
         shape.Transform!.Scale = new Vector3(scaleX, scaleY, scaleZ);
         $"📏 Shape '{name}' scaled to ({scaleX:F2}x, {scaleY:F2}x, {scaleZ:F2}x)".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }

   [Description("Changes the dimensions (width, height, depth) of an existing shape")]
   public List<ShapeInfo> ChangeShapeDimensions(
      [Description("The name of the shape")] string name,
      [Description("New width (X dimension)")] double width,
      [Description("New height (Y dimension)")] double height,
      [Description("New depth (Z dimension)")] double depth)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));

      if (shape != null)
      {
         shape.Width = width;
         shape.Height = height;
         shape.Depth = depth;
         $"📐 Shape '{name}' resized to {width:F1} x {height:F1} x {depth:F1}".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }

   [Description("Changes the visibility/active state of a shape")]
   public List<ShapeInfo> ChangeState(
      [Description("The name of the shape")] string name, 
      [Description("Whether the shape should be visible/active")] bool isOn)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));

      if (shape != null)
      {
         shape.IsOn = isOn;
         $"👁️ Shape '{name}' is now {(isOn ? "visible" : "hidden")}".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }

   [Description("Changes the color of a shape")]
   public List<ShapeInfo> ChangeColor(
      [Description("The name of the shape")] string name, 
      [Description("The new color for the shape")] string color)
   {
      var stage = EstablishGeometryStage();
      var shape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(name));

      if (shape != null)
      {
         shape.Color = color;
         $"🎨 Changed '{name}' color to '{color}'".WriteSuccess();
      }
      else
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }

   [Description("Duplicates an existing shape with a new name and optional position offset")]
   public List<ShapeInfo> DuplicateShape(
      [Description("The name of the shape to duplicate")] string sourceName,
      [Description("Name for the new duplicated shape")] string newName,
      [Description("X offset from original position")] double offsetX,
      [Description("Y offset from original position")] double offsetY,
      [Description("Z offset from original position")] double offsetZ)
   {
      var stage = EstablishGeometryStage();
      var sourceShape = stage.Members<GeometryShape>().FirstOrDefault(s => s.GetName().Matches(sourceName));

      if (sourceShape != null)
      {
         var pos = sourceShape.Transform?.Position ?? Vector3.Zero;
         
         var newShape = new GeometryShape(newName, sourceShape.GeomType, sourceShape.Width, sourceShape.Height, sourceShape.Depth)
         {
            IsOn = sourceShape.IsOn,
            Color = sourceShape.Color
         };
         
         newShape.Transform!.Position = new Vector3(pos.X + offsetX, pos.Y + offsetY, pos.Z + offsetZ);
         
         if (sourceShape.Transform?.Rotation != null)
         {
            newShape.Transform.Rotation = sourceShape.Transform.Rotation;
         }
         
         if (sourceShape.Transform?.Scale != null)
         {
            newShape.Transform.Scale = sourceShape.Transform.Scale;
         }
         
         stage.AddShape(newShape);
         
         $"📋 Duplicated '{sourceName}' as '{newName}' with offset ({offsetX:F1}, {offsetY:F1}, {offsetZ:F1})".WriteSuccess();
      }
      else
      {
         $"⚠️  Source shape '{sourceName}' not found".WriteWarning();
      }

      RefreshUI();
      return GetShapes();
   }
}

public class GeometryShape : FoShape3D
{

   public bool? IsOn { get; set; }  = false;

   public string Status() => $"{(IsOn == true ? "visible" : "hidden")}";

   public GeometryShape(string name, string shapeType = "box", double? width = null, double? height = null, double? depth = null) : base(name)
   {
      var gen = new MockDataGenerator();

      // Use provided dimensions or defaults
      var w = width ?? 2.0;
      var h = height ?? 2.0;
      var d = depth ?? 2.0;

      // Create shape based on type
      switch (shapeType.ToLower())
      {
         case "sphere":
            CreateSphere(name, w, h, d);
            break;
         case "cylinder":
            CreateCylinder(name, w, h, d);
            break;
         case "cone":
            CreateCone(name, w, h, d);
            break;
         case "torus":
            CreateTorus(name, w, h, d);
            break;
         case "tetrahedron":
            CreateTetrahedron(name, w, h, d);
            break;
         case "octahedron":
            CreateOctahedron(name, w, h, d);
            break;
         case "dodecahedron":
            CreateDodecahedron(name, w, h, d);
            break;
         case "icosahedron":
            CreateIcosahedron(name, w, h, d);
            break;
         case "torusknot":
            CreateTorusKnot(name, w, h, d);
            break;
         case "capsule":
            CreateCapsule(name, w, h, d);
            break;
         case "plane":
            CreatePlane(name, w, h, d);
            break;
         case "circle":
            CreateCircle(name, w, h, d);
            break;
         case "ring":
            CreateRing(name, w, h, d);
            break;
         case "box":
         default:
            CreateBox(name, w, h, d);
            break;
      }

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
      AddShape(tag); 
      GetTreeNodeTitle().WriteSuccess();
   }

   public override string GetTreeNodeTitle()
   {
      var pos = Transform!.Position;
      return $"{GetName()} {Color} is {Status()} @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
   }
}