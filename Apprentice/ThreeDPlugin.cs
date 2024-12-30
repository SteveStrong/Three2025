using System.ComponentModel;
using System.Text.Json.Serialization;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.SemanticKernel;

namespace Three2025.Apprentice;

public class ThreeDPlugin
{

   private IFoundryService Foundry;
   public ThreeDPlugin(IFoundryService service)
   {
      Foundry = service;
   }

   [KernelFunction("CreateBlock")]
   [Description("Create a block 1,2,3")]
   public async void CreateBlock()
   {
      await Task.CompletedTask;
      $"Creating block".WriteSuccess();

      // var world = Foundry.WorldManager().AllWorlds().FirstOrDefault();
      // if ( world == null)
      // {
      //    $"No world found".WriteError();
      //    return;
      // }

      var Arena = Foundry.Arena();
      var block = new FoShape3D("Block");
      block.CreateBox("Test",1,2,3);
      Arena.AddShape<FoShape3D>(block);
            $"Creating block Added".WriteSuccess();

   }

  
}

