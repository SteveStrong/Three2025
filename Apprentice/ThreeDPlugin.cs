using System.ComponentModel;
using System.Text.Json.Serialization;
using FoundryRulesAndUnits.Extensions;
using Microsoft.SemanticKernel;

namespace Three2025.Apprentice;

public class ThreeDPlugin
{

   [KernelFunction("CreateBlock")]
   [Description("Create a block 1,2,3")]
   public async void CreateBlock()
   {
        await Task.CompletedTask;

   }

  
}

