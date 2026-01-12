#nullable enable

using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Tests;
using FoundryMentorModeler.Evaluator;
using FoundryMentorModeler.Persistence;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Extensions;
using FoundryCore;
using FoundryRulesAndUnits.Models;

namespace Three2025.Components.Pages;

public partial class ReductionOperators : ComponentBase
{
    [Inject] public required IMentorServices MentorServices { get; set; }
    [Inject] public required IModelEditor ModelEditor { get; set; }
    [Inject] public required ComponentBus PubSub { get; set; }

    private KnModel _model = null!;
    private ITreeNode? _selectedItem = null;

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(100);
        
        // Subscribe to model changes to refresh when parameters are smashed
        PubSub.Subscribe<RefreshRenderMessage>(msg => {
            InvokeAsync(StateHasChanged);
        });
        
        SetupTestComponents();
    }

    private void HandleItemSelected(ITreeNode item)
    {
        _selectedItem = item;
        StateHasChanged();
    }

    private string GetStatusBadgeClass(CalculationStatus status)
    {
        return status switch
        {
            CalculationStatus.Success => "bg-success",
            CalculationStatus.Failure => "bg-danger",
            CalculationStatus.NotCalculated => "bg-secondary",
            _ => "bg-secondary"
        };
    }

    private string GetStatusText(CalculationStatus status)
    {
        return status switch
        {
            CalculationStatus.Success => "✓ Success",
            CalculationStatus.Failure => "✗ Failure",
            CalculationStatus.NotCalculated => "— Not Calculated",
            _ => "Unknown"
        };
    }

    private string? GetExpectedValue(KnParameter param)
    {
        // Try to extract expected value based on parameter name
        return param.Name switch
        {
            "sum" when param.GetKnParent()?.Name == "BoxWithManyValues" => "125.0",
            "count" when param.GetKnParent()?.Name == "BoxWithManyValues" => "5",
            "avg" when param.GetKnParent()?.Name == "BoxWithManyValues" => "25.0",
            "min" when param.GetKnParent()?.Name == "BoxWithManyValues" => "5.0",
            "max" when param.GetKnParent()?.Name == "BoxWithManyValues" => "45.0",
            "first" when param.GetKnParent()?.Name == "BoxWithManyValues" => "5.0",
            "last" when param.GetKnParent()?.Name == "BoxWithManyValues" => "45.0",
            "total" when param.GetKnParent()?.Name == "BoxWithDimensions" => "60.0",
            "count" when param.GetKnParent()?.Name == "BoxWithDimensions" => "3",
            "average" when param.GetKnParent()?.Name == "BoxWithDimensions" => "20.0",
            "total" when param.GetKnParent()?.Name == "BoxWithSingleValue" => "42.0",
            "average" when param.GetKnParent()?.Name == "BoxWithSingleValue" => "42.0",
            "min" when param.GetKnParent()?.Name == "BoxWithSingleValue" => "42.0",
            "max" when param.GetKnParent()?.Name == "BoxWithSingleValue" => "42.0",
            "count" when param.GetKnParent()?.Name == "BoxWithEmptyList" => "0",
            "sum" when param.GetKnParent()?.Name == "BoxWithEmptyList" => "0",
            _ => null
        };
    }

    private void ExecuteAction(KnParameter param, dynamic action)
    {
        if (action is KnTreeNodeAction knAction)
        {
            knAction.Action.Invoke(MentorServices);
        }
        else if (action is TreeNodeAction treeAction)
        {
            treeAction.Action.Invoke();
        }
        StateHasChanged();
    }

    private void SetupTestComponents()
    {
        try
        {
            if (MentorServices == null)
                return;

            // Use EstablishModel to properly register with MentorServices
            _model = MentorServices.EstablishModel<AnimatedKnModel>("ReductionTests");
            _model.SetExpanded(true);

            // Add test components
            AddTestComponent(_model, new BoxWithDimensions("BoxWithDimensions"));
            AddTestComponent(_model, new BoxWithEmptyList("BoxWithEmptyList"));
            AddTestComponent(_model, new BoxWithSingleValue("BoxWithSingleValue"));
            AddTestComponent(_model, new BoxWithManyValues("BoxWithManyValues"));
            AddTestComponent(_model, new BoxWithLengthMeasures("BoxWithLengthMeasures"));
            
            // Signal tree to refresh
            PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR during setup: {ex.Message}");
        }
    }

    private void AddTestComponent(KnModel model, PartComponent component)
    {
        try
        {
            // Add via ModelEditor (triggers events and proper tree registration)
            ModelEditor.AddChild(model, component);
            component.SetExpanded(true);  // Expand to show parameters
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add {component.Name}: {ex.Message}");
        }
    }
}
