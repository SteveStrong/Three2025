using System.ComponentModel;
using System.Text.Json.Serialization;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Microsoft.SemanticKernel;

#nullable enable
namespace Three2025.Apprentice;

public class ThreeDPlugin
{

   private IFoundryService Foundry;
   private MockDataGenerator DataGenerator { get; set; } = new();
   public ThreeDPlugin(IFoundryService service)
   {
      Foundry = service;
   }

   private FoShape3D LoadIntoArena(FoShape3D shape)
   {
      var arena = Foundry.Arena();
      var stage = arena.EstablishStage<FoStage3D>("Main Stage");

      arena.AddShapeToStage<FoShape3D>(shape);  //this is what the world publish is doing

      //stage.PreRender(arena);

      // var (found, scene) = arena.CurrentScene();
      // if (found)
      //    stage.RefreshScene(scene);

      return shape;
   }

   [KernelFunction("CreateBlockShape")]
   [Description("Create a block shape with a random color")]
   public void CreateBlockShape(string Color)
   {

      $"Creating block".WriteSuccess();


      var block = new FoShape3D("Block")
      {
         Color = Color
      };
        
      var name = DataGenerator.GenerateWord();
      var w = DataGenerator.GenerateDouble(2, 10);
      var h = DataGenerator.GenerateDouble(2, 10);
      var d = DataGenerator.GenerateDouble(2, 10);
      block.CreateBox(name,w,h,d);

      LoadIntoArena(block);
   }

   [KernelFunction("PickARandomColor")]
   [Description("Generate a Random Color")]
   [return: Description("a Color as a string")]
   public string PickARandomColor()
   {
      var color = DataGenerator.GenerateColor();
      return color;
   }

   [KernelFunction("FindShapeByName")]
   [Description("find a Block by name")]
   [return: Description("FoShape3D or nothing")]
   public FoShape3D? FindShapeByName(string name)
   {
      var Arena = Foundry.Arena();
      var Stage = Arena.CurrentStage();
      var result = Stage.FindShape<FoShape3D>(name);
      return result.success ? result.found : null;
   }

   [KernelFunction("FindShapeByColor")]
   [Description("find a Block by name")]
   [return: Description("List of all the shapes that match the color")]
   public List<FoShape3D> FindShapeByColor(string color)
   {
      var Arena = Foundry.Arena();
      var Stage = Arena.CurrentStage();
      var shapes = Stage.Members<FoShape3D>().Where(x => x.Color == color).ToList();
      return shapes;
   }

    [KernelFunction("GetAllShapes")]
    [Description("Get a list of all the blocks")]
    [return: Description("List of all the shapes")]
    public List<FoShape3D> GetAllTheShapes()
    {
        var Arena = Foundry.Arena();
        var Stage = Arena.CurrentStage();
        var shapes = Stage.Members<FoShape3D>();

        foreach (var item in shapes)
        {
            $"Shape: {item.Name} Type: {item.Type} Color: {item.Color}".WriteSuccess();
        }
        return shapes;
    }

    [KernelFunction("GetShapeColor")]
   [Description("get the color of a Block")]
   [return: Description("color as a string")]
   public async Task<string> GetShapeColor(FoShape3D Block)
   {
      await Task.CompletedTask;
      return Block.Color;
   }

   [KernelFunction("SetShapeColor")]
   [Description("set the color of a Block")]
   [return: Description("color as a string")]
   public FoShape3D SetShapeColor(FoShape3D Block, string color)
   {
      Block.Color = color;
      return Block;
   }
   [KernelFunction("MoveBlockLeftOrRight")]
   [Description("Moves a block in the x direction, Left is negative, Right is positive")]
   [return: Description("Return a block that was moved")]
   public FoShape3D MoveBlockLeftOrRight(FoShape3D Block, int dx)
   {
      var pos = Block.Transform.Position;
      pos.X += dx;
      //Block.MoveTo(pos.X, pos.Y, pos.Z);
      return Block;
   }
  
}

