using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

using FoundryWorldsAndDrawings.ThreeD.Maths;


namespace Three2025.Apprentice;
#nullable enable


public interface ILightingTech : ITechnician
{
   void SetStage(FoStage3D stage);

   FoStage3D EstablishLightingStage();

   void ClearLights();

   void SaveLights();

   void RestoreLights();

   string PickARandomColor();

   List<LightingComponent> GetLights();

   List<LightingComponent> AddLight(string name, bool isOn, string color);

   LightingComponent? DeleteLight(string name);

   LightingComponent? RepositionLight(string name, double x, double y, double z);

   LightingComponent? ChangeState(string name, bool isOn);

   LightingComponent? ChangeColor(string name, string color);
}

public class LightingTech :ILightingTech
{

   private IWorkspace Workspace;

   private FoStage3D? Stage { get; set; }


   private MockDataGenerator DataGenerator { get; set; } = new();



   public LightingTech(IWorkspace workspace)
   {
      Workspace = workspace;
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
      //FoundryServices.PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());
   }
   
   [Description("Establish a Lighting Model for the application")]
   public FoStage3D EstablishLightingStage()
   {

      if ( Stage != null)
         return Stage;

      var arena = Workspace.GetArena();
      Stage = arena.EstablishStage<FoStage3D>("Lighting");



      // var lights = new List<LightingComponent>()
      // {
      //    new LightingComponent("Table Lamp") { IsOn = false, Color = DataGenerator.GenerateColor() },
      //    new LightingComponent("Porch light") { IsOn = false, Color = DataGenerator.GenerateColor() },
      //    new LightingComponent("Chandelier") { IsOn = true, Color = DataGenerator.GenerateColor() }
      // };

      // foreach (var light in lights)
      // {
      //    arena.AddShapeToStage<LightingComponent>(light);
      // }


      RefreshUI();
      return Stage;
   }

   [Description("Clears the list of lights")]
   public void ClearLights()
   {
      var stage = EstablishLightingStage();
      _ = stage.ClearAll();
      RefreshUI();
   }

   [Description("saves a list of lights to a file")]
   public void SaveLights()
   {  
      var stage = EstablishLightingStage();
      var lights = stage.Members<LightingComponent>();
      var data = CodingExtensions.DehydrateList<LightingComponent>(lights,false);
      FileHelpers.WriteData("Data", "lights.json", data);
   }

   [Description("restores a list of lights from a file")]
   public void RestoreLights()
   {
      var data = FileHelpers.ReadData("Data", "lights.json");
      var list = CodingExtensions.HydrateList<LightingComponent>(data,false);
      
   
      var stage = EstablishLightingStage();
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

   [Description("Gets a list of lights and their current state")]
   public List<LightingComponent> GetLights()
   {
      var stage = EstablishLightingStage();
      return stage.Members<LightingComponent>();
   }
   
   [Description("Create and add a light")]
   public List<LightingComponent> AddLight(
      [Description("The name of the light to create")] string name, 
      [Description("Whether the light should be on or off")] bool isOn, 
      [Description("The color of the light")] string color)
   {
         var stage = EstablishLightingStage();

         var newLight = new LightingComponent(name)
         {
            IsOn = isOn,
            Color = color
         };

         stage.AddShape(newLight);

         RefreshUI();

         return stage.Members<LightingComponent>();
   }


   [Description("delete a light")]
   public LightingComponent? DeleteLight(
      [Description("The name of the light to delete")] string name)
   {
         var stage = EstablishLightingStage();
         var light = stage.Members<LightingComponent>().FirstOrDefault(light => light.GetName().Matches(name));

         if (light != null)
            light.DeleteFromStage(stage);

         RefreshUI();

         return light;
   }

   [Description("Changes the X, Y, Z position of the light")]
   public LightingComponent? RepositionLight(
      [Description("The name of the light to reposition")] string name, 
      [Description("The X coordinate")] double x, 
      [Description("The Y coordinate")] double y, 
      [Description("The Z coordinate")] double z)
   {
      var list = GetLights();
      var light = list.FirstOrDefault(light => light.GetName().Matches(name));

      if ( light != null)
      {
         light.Transform!.Position = Vector3.Zero;
         light.Transform.MoveBy(x, y, z);
         $"Light {name} repositioned to {x}, {y}, {z}".WriteSuccess();
      }


      RefreshUI();
      return light;
   }

   [Description("Changes the state of the light")]
   public LightingComponent? ChangeState(
      [Description("The name of the light")] string name, 
      [Description("Whether the light should be on or off")] bool isOn)
   {
      var list = GetLights();
      var light = list.FirstOrDefault(light => light.GetName().Matches(name));

      if ( light != null)
         light.IsOn = isOn;

      RefreshUI();
      return light;
   }

   [Description("Changes the color of the light")]
   public LightingComponent? ChangeColor(
      [Description("The name of the light")] string name, 
      [Description("The new color for the light")] string color)
   {
      var list = GetLights();
      var light = list.FirstOrDefault(light => light.GetName().Matches(name));

      if ( light != null)
         light.Color = color;

      RefreshUI();
      return light;
   }
}

public class LightingComponent : FoShape3D
{

   public bool? IsOn { get; set; }  = false;

   public string Status() => $"{(IsOn == true ? "on" : "off")}";

   public LightingComponent(string name) : base(name)
   {
      var gen = new MockDataGenerator();

      CreateBox(name, 2, gen.GenerateDouble(1,5), gen.GenerateDouble(1,5));

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