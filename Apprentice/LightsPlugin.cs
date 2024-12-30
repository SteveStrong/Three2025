using System.ComponentModel;
using System.Text.Json.Serialization;
using FoundryRulesAndUnits.Extensions;
using Microsoft.SemanticKernel;

namespace Three2025.Apprentice;
#nullable enable

public class LightsPlugin
{
   // Mock data for the lights
   private readonly List<LightModel> lights = new()
   {
      new LightModel { Id = 1, Name = "Table Lamp", IsOn = false },
      new LightModel { Id = 2, Name = "Porch light", IsOn = false },
      new LightModel { Id = 3, Name = "Chandelier", IsOn = true }
   };

   [KernelFunction("clear_lights")]
   [Description("Clears the list of lights")]
   public async void ClearLightsAsync()
   {
        await Task.CompletedTask;
        lights.Clear();
   }

   [KernelFunction("Save_Lights")]
   [Description("saves a list of lights to a file")]
   public async void SaveLightsAsync()
   {
      await Task.CompletedTask;
      var data = CodingExtensions.DehydrateList<LightModel>(lights,false);
      FileHelpers.WriteData("Data", "lights.json", data);
   }

   [KernelFunction("Restore_Lights")]
   [Description("restores a list of lights from a file")]
   public async void RestoreLightsAsync()
   {
      await Task.CompletedTask;
      var data = FileHelpers.ReadData("Data", "lights.json");
      var list = CodingExtensions.HydrateList<LightModel>(data,false);
      lights.Clear();
      lights.AddRange(list);
   }

   [KernelFunction("get_lights")]
   [Description("Gets a list of lights and their current state")]
   [return: Description("An array of lights")]
   public async Task<List<LightModel>> GetLightsAsync()
   {
        await Task.CompletedTask;
        return lights;
   }
   
   [KernelFunction("add_light")]
   [Description("Create and add a light")]
   [return: Description("An array of lights")]
   public async Task<List<LightModel>> AddLight(string name, bool isOn)
   {
        await Task.CompletedTask;
        var newLight = new LightModel
        {
            Id = lights.Count + 1,
            Name = name,
            IsOn = isOn
        };

        lights.Add(newLight);

        return lights;
   }

   [KernelFunction("report_light_in_on")]
   [Description("report that a light is on")]
   public async void ReportLightIsOn(LightModel light)
   {
        await Task.CompletedTask;
        $"{light.Name} is {light.Status()}".WriteSuccess();
   }

   [KernelFunction("report_light_in_off")]
   [Description("report that a light is off")]
   public async void ReportLightIsOff(LightModel light)
   {
        await Task.CompletedTask;
        $"{light.Name} is {light.Status()}".WriteError();
   }

   [KernelFunction("ReportAllLights")]
   [Description("report the status of a every light")]
   [return: Description("An array of lights")]
   public async Task<List<LightModel>> ReportAllLights()
   {
      await Task.CompletedTask;
      foreach (var item in lights)
      {
         ReportStatus(item);
      }
      return lights;
   }

   [KernelFunction("ReportStatus")]
   [Description("report the status of a light")]
   public async void ReportStatus(LightModel light)
   {
      await Task.CompletedTask;
      if ( light.IsOn == true)
         ReportLightIsOn(light);
      else
         ReportLightIsOff(light);
   }

   [KernelFunction("change_state")]
   [Description("Changes the state of the light")]
   [return: Description("The updated state of the light; will return null if the light does not exist")]
   public async Task<LightModel?> ChangeStateAsync(int id, bool isOn)
   {
        await Task.CompletedTask;
        var light = lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
        {
            return null;
        }

        // Update the light with the new state
        light.IsOn = isOn;

        return light;
   }
}

public class LightModel
{
   [JsonPropertyName("id")]
   public int Id { get; set; } = 0;

   [JsonPropertyName("name")]
   public string Name { get; set; } = string.Empty;

   [JsonPropertyName("is_on")]
   public bool? IsOn { get; set; }  = false;

   public string Status() => $"{(IsOn == true ? "on" : "off")}";
}