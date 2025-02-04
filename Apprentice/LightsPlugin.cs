using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Web;
using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

using Microsoft.SemanticKernel;


namespace Three2025.Apprentice;
#nullable enable



public class LightingShapes
{
   private IFoundryService FoundryServices;
   private IWorkspace Workspace;

   private FoStage3D? Stage { get; set; }


   private MockDataGenerator DataGenerator { get; set; } = new();



   public LightingShapes(IWorkspace workspace, IFoundryService mentor)
   {
      Workspace = workspace;
      FoundryServices = mentor;
   }

   [KernelFunction("RefreshUI")]
   [Description("Send a message to refresh the TreeView")]
   public void RefreshUI()
   {
      //FoundryServices.PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());
   }
   
   [KernelFunction("EstablishLightingStage")]
   [Description("Establish a Lighting Model for the lights")]
   [return: Description("LightingModel  The container for all the lights")]
   public FoStage3D EstablishLightingStage()
   {

      if ( Stage != null)
         return Stage;

      var arena = Workspace.GetArena();
      Stage = arena.EstablishStage<FoStage3D>("Lighting");



      var lights = new List<LightingComponent>()
      {
         new LightingComponent("Table Lamp") { IsOn = false, Color = DataGenerator.GenerateColor() },
         new LightingComponent("Porch light") { IsOn = false, Color = DataGenerator.GenerateColor() },
         new LightingComponent("Chandelier") { IsOn = true, Color = DataGenerator.GenerateColor() }
      };

      foreach (var light in lights)
      {
         arena.AddShapeToStage<LightingComponent>(light);
      }


      RefreshUI();
      return Stage;
   }

   [KernelFunction("clear_lights")]
   [Description("Clears the list of lights")]
   public void ClearLights()
   {
      var stage = EstablishLightingStage();
      stage.ClearStage();
      RefreshUI();
   }

   [KernelFunction("Save_Lights")]
   [Description("saves a list of lights to a file")]
   public void SaveLights()
   {
      var stage = EstablishLightingStage();
      var lights = stage.Members<LightingComponent>();
      var data = CodingExtensions.DehydrateList<LightingComponent>(lights,false);
      FileHelpers.WriteData("Data", "lights.json", data);
   }

   [KernelFunction("Restore_Lights")]
   [Description("restores a list of lights from a file")]
   public void RestoreLights()
   {
      ClearLights();
      var data = FileHelpers.ReadData("Data", "lights.json");
      var list = CodingExtensions.HydrateList<LightingComponent>(data,false);
      
   
      var stage = EstablishLightingStage();
      stage.ClearStage();

      var arena = Workspace.GetArena();
      foreach (var item in list)
      {
         arena.AddShapeToStage<LightingComponent>(item);
      }
      RefreshUI();

   }

   [KernelFunction("PickARandomColor")]
   [Description("Generate a Random Color")]
   [return: Description("a Color as a string")]
   public string PickARandomColor()
   {
      var color = DataGenerator.GenerateColor();
      return color;
   }

   [KernelFunction("get_lights")]
   [Description("Gets a list of lights and their current state")]
   [return: Description("An array of lights")]
   public List<LightingComponent> GetLights()
   {
      var stage = EstablishLightingStage();
      return stage.Members<LightingComponent>();
   }
   
   [KernelFunction("add_light")]
   [Description("Create and add a light")]
   [return: Description("An array of lights")]
   public List<LightingComponent> AddLight(string name, bool isOn, string color)
   {
         var stage = EstablishLightingStage();
         var list = stage.Members<LightingComponent>();

         var newLight = new LightingComponent(name)
         {
            IsOn = isOn,
            Color = color
         };

         var arena = Workspace.GetArena();
         arena.AddShapeToStage<LightingComponent>(newLight);

         RefreshUI();

         return stage.Members<LightingComponent>();
   }


   [KernelFunction("delete_light")]
   [Description("delete a light")]
   [return: Description("return the deleted light")]
   public LightingComponent? DeleteLight(string name)
   {
         var stage = EstablishLightingStage();
         var list = stage.Members<LightingComponent>();

         var light = list.FirstOrDefault(light => light.GetName().Matches(name));

         if (light != null)
            light.DeleteFromStage(stage);

         RefreshUI();

         return light;
   }

   [KernelFunction("Reposition_Light")]
   [Description("Changes the X, Y, Z position of the light")]
   [return: Description("The updated position of the light; will return null if the light does not exist")]
   public LightingComponent? RepositionLight(string name, double x, double y, double z)
   {
      var list = GetLights();
      var light = list.FirstOrDefault(light => light.GetName().Matches(name));

      if ( light != null)
      {
         light.Transform.Position = new Vector3(x, y, z);
         $"Light {name} repositioned to {x}, {y}, {z}".WriteSuccess();
         light.SetDirty(true);
      }


      RefreshUI();
      return light;
   }

   [KernelFunction("change_state")]
   [Description("Changes the state of the light")]
   [return: Description("The updated state of the light; will return null if the light does not exist")]
   public LightingComponent? ChangeStateAsync(string name, bool isOn)
   {
      var list = GetLights();
      var light = list.FirstOrDefault(light => light.GetName().Matches(name));

      if ( light != null)
         light.IsOn = isOn;

      RefreshUI();
      return light;
   }

   [KernelFunction("change_color")]
   [Description("Changes the color of the light")]
   [return: Description("The updated color of the light; will return null if the light does not exist")]
   public LightingComponent? ChangeColorAsync(string name, string color)
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

      CreateBox(name, gen.GenerateDouble(1,5), gen.GenerateDouble(1,5), gen.GenerateDouble(1,5));

      var tag = new FoText3D("tag")
      {
         Text = name,
         FontSize = 0.5,
         Transform = new Transform3()
         {
            Position = new Vector3(1, 0, 0),
         },
         Color = "black"
      };
      AddSubGlyph3D(tag); 
      GetTreeNodeTitle().WriteSuccess();
   }

   public override string GetTreeNodeTitle()
   {
      return $"{GetName()} {Color} is {Status()}";
   }
}