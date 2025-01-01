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

   [KernelFunction("CreateBlock")]
   [Description("Create a block 1,2,3")]
   public async void CreateBlock(string Color)
   {

      $"Creating block".WriteSuccess();

      var Arena = Foundry.Arena();
      var block = new FoShape3D("Block")
      {
         Color = Color
      };
        
      var name = DataGenerator.GenerateWord();
      var w = DataGenerator.GenerateDouble(2, 10);
      var h = DataGenerator.GenerateDouble(2, 10);
      var d = DataGenerator.GenerateDouble(2, 10);
      block.CreateBox(name,w,h,d);
      Arena.AddShape<FoShape3D>(block);
      await Arena.UpdateArena();
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
      var Block = Stage.GetShapes3D().FirstOrDefault(x => x.Name == name);
      return Block;
   }

   [KernelFunction("FindShapeByColor")]
   [Description("find a Block by name")]
   [return: Description("FoShape3D or nothing")]
   public List<FoShape3D> FindShapeByColor(string color)
   {
      var Arena = Foundry.Arena();
      var Stage = Arena.CurrentStage();
      var shapes = Stage.GetShapes3D().Where(x => x.Color == color).ToList();
      return shapes;
   }

   [KernelFunction("GetShapeColor")]
   [Description("get the color of a Block")]
   [return: Description("color as a string")]
   public string GetShapeColor(FoShape3D Block)
   {
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
      var pos = Block.Position!;
      pos.X += dx;
      Block.MoveTo(pos.X, pos.Y, pos.Z);
      return Block;
   }
  
}

