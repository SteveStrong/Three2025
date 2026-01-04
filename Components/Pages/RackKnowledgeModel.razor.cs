using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryMentorModeler.Persistence;
using FoundryWorldsAndDrawings.PubSub;
using FoundryWorldsAndDrawings.Solutions;
using Three2025.Apprentice.RackEquipment;
using FoundryWorldsAndDrawings.Shape;
using FoundryCore;
using FoundryRulesAndUnits.Models;

namespace Three2025.Components.Pages;

public partial class RackKnowledgeModel : ComponentBase
{
    [Inject] public IMentorServices MentorServices { get; set; } = null!;
    [Inject] public IModelEditor ModelEditor { get; set; } = null!;
    [Inject] public ComponentBus PubSub { get; set; } = null!;

    private KnModel _model = null;
    private DataCenterRackModel _dataCenterModel = null;
    private FoStage3D _generatedStage = null;
    private ITreeNode _selectedItem = null;
    private bool _isLoading = false;
    private string _statusMessage = null;
    private bool _isError = false;

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(100);
        
        // Subscribe to model changes
        PubSub.Subscribe<RefreshRenderMessage>(msg => {
            InvokeAsync(StateHasChanged);
        });
    }

    private void HandleItemSelected(ITreeNode item)
    {
        _selectedItem = item;
        StateHasChanged();
    }

    private async Task CreateDataCenterModel()
    {
        _isLoading = true;
        _statusMessage = null;
        StateHasChanged();

        try
        {
            await Task.Delay(50); // UI update

            // Create KnModel to hold our knowledge model
            _model = MentorServices.EstablishModel<AnimatedKnModel>("RackDataCenter");
            _model.SetExpanded(true);

            // Create the data center knowledge model
            _dataCenterModel = new DataCenterRackModel("MF_DataCenter");
            _dataCenterModel.SetExpanded(true);

            // Add to model via ModelEditor
            ModelEditor.AddChild(_model, _dataCenterModel);

            // Expand all cabinets to show equipment
            var cabinets = _dataCenterModel.ModelComponents<RackCabinetConcept>();
            foreach (var cabinet in cabinets)
            {
                cabinet.SetExpanded(true);
                
                // Expand equipment too
                var equipment = cabinet.ModelComponents<RackEquipmentConcept>();
                foreach (var equip in equipment)
                {
                    equip.SetExpanded(true);
                }
            }

            var totalCabinets = _dataCenterModel.FindParameter("TotalCabinets")?.GetValue().AsNumber() ?? 0;
            var totalEquip = _dataCenterModel.FindParameter("TotalEquipment")?.GetValue().AsNumber() ?? 0;
            var totalRU = _dataCenterModel.FindParameter("TotalRUUsed")?.GetValue().AsNumber() ?? 0;

            _statusMessage = $"✅ Created knowledge model: {totalCabinets} cabinets, {totalEquip} equipment items, {totalRU} RU used";
            _isError = false;

            // Signal tree to refresh
            await Task.Run(() => PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null)));
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error creating model: {ex.Message}";
            _isError = true;
            Console.WriteLine($"ERROR: {ex}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task GenerateFOGeometry()
    {
        if (_dataCenterModel == null)
        {
            _statusMessage = "⚠️ Create knowledge model first";
            _isError = true;
            StateHasChanged();
            return;
        }

        _isLoading = true;
        _statusMessage = null;
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            // Get stage from arena (delegate to modeling layer)
            var arena = MentorServices.EstablishArena();
            _generatedStage = arena.EstablishStage<FoStage3D>("DataCenter");
            
            // Generate FO geometry into provided stage (technician operates on stage, doesn't create it)
            RackKnowledgeToFoFactory.GenerateDataCenter(_dataCenterModel, _generatedStage);

            var shapeCount = _generatedStage.GetMembers<RackCabinetShape>().Count();
            _statusMessage = $"✅ Generated {shapeCount} FO shapes from knowledge model";
            _isError = false;
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error generating geometry: {ex.Message}";
            _isError = true;
            Console.WriteLine($"ERROR: {ex}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void RefreshCalculations()
    {
        if (_dataCenterModel == null) return;

        try
        {
            // Force recalculation by accessing parameters
            var cabinets = _dataCenterModel.ModelComponents<RackCabinetConcept>();
            foreach (var cabinet in cabinets)
            {
                var usedRU = cabinet.FindParameter("UsedRU")?.GetValue();
                var availRU = cabinet.FindParameter("AvailableRU")?.GetValue();
            }

            _statusMessage = "✅ Calculations refreshed";
            _isError = false;
            
            PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null));
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error refreshing: {ex.Message}";
            _isError = true;
        }
    }

    private void ClearModel()
    {
        _model = null;
        _dataCenterModel = null;
        _generatedStage = null;
        _selectedItem = null;
        _statusMessage = "🗑️ Model cleared";
        _isError = false;
        StateHasChanged();
    }
}
