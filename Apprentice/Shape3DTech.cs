#nullable enable

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
      var result = ShapeEditor.GetAllShapes();
      if (result.IsError())
      {
         $"❌ Failed to get shapes: {result.Display()}".WriteError();
         return;
      }
      // Use Value() method and cast to List<FoShape3D>
      var shapeList = result.Value() as List<FoShape3D> ?? new List<FoShape3D>();
      var shapes = shapeList.ToList(); // Now work with all FoShape3D objects
      var data = CodingExtensions.DehydrateList<FoShape3D>(shapes, false);
      FileHelpers.WriteData("Data", "shapes.json", data);
   }

   [Description("Restores shapes from a saved file")]
   public void RestoreShapes()
   {
      var data = FileHelpers.ReadData("Data", "shapes.json");
      var list = CodingExtensions.HydrateList<FoShape3D>(data, false);

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
      var result = ShapeEditor.GetShapeByName(name);
      if (result.IsError())
      {
         $"⚠️  Shape '{name}' not found".WriteWarning();
         return null;
      }
      $"🔍 Found shape '{name}'".WriteInfo();
      return result.AsShape3D();
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
      shape.ComputeTreeNodeTitle = TreeNodeFormatters.Spatial;
      
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
   public FoShape3D AddShape(
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

      return newShape;
   }

   [Description("Create and add a 3D shape with specific dimensions")]
   public FoShape3D AddShapeWithDimensions(
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

      return newShape;
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
      var shape = GetShapeByName(name);
      if (shape == null)
         return OPResult.Error($"Shape '{name}' not found");

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
      var result = ShapeEditor.SetPosition(name, endPos);
      return result.IsError() 
         ? result 
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
      return ShapeEditor.SetColor(name, color);
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
   public string GetChildShapes(
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
