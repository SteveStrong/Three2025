using System.ComponentModel;
using FoundryMentorModeler.Model;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Components.Pages;
using Three2025.Models.Apprentice;
using Three2025.Services.Agents;
using FoundryMentorModeler.Evaluator;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Interface for model manipulation operations with context tracking
/// </summary>
public interface IModelTech : ITechnician
{
    /// <summary>
    /// Current working model - set by EstablishModel or SetCurrentModel
    /// </summary>
    KnModel? CurrentModel { get; }
    
    /// <summary>
    /// Current working component - set by AddComponent or SetCurrentComponent
    /// </summary>
    KnComponent? CurrentComponent { get; }

    void ClearModel();
    
    /// <summary>
    /// Create or get existing model and set as current
    /// </summary>
    KnModel EstablishModel(string modelName, string? modelType = null);
    
    /// <summary>
    /// Set current working model
    /// </summary>
    KnModel? SetCurrentModel(string? modelName);
    
    /// <summary>
    /// Set current working component by path
    /// </summary>
    KnComponent? SetCurrentComponent(string? componentPath);
    
    /// <summary>
    /// List all available models
    /// </summary>
    List<KnModel> ListModels();
    
    /// <summary>
    /// Add a component to current model (or specified model), becomes current component
    /// </summary>
    KnComponent AddComponent(string componentName, string? modelName = null, string? parentComponentPath = null);
    
    /// <summary>
    /// Remove current component (or specified component)
    /// </summary>
    bool RemoveComponent(string? componentPath = null);
    
    /// <summary>
    /// Set parameter on current component (or specified component)
    /// </summary>
    KnParameter SetParameter(string parameterName, string value, string? componentPath = null);
    
    /// <summary>
    /// Get parameter from current component (or specified component)
    /// </summary>
    KnParameter? GetParameter(string parameterName, string? componentPath = null);
    
    /// <summary>
    /// List all components in current model (or specified model)
    /// </summary>
    List<KnComponent> ListComponents(string? modelName = null);
    
    /// <summary>
    /// Set the page context for visual shape creation
    /// </summary>
    void SetPageContext(FoPage2D canvasPage, IMentorStudio mentorStudio);
    
    /// <summary>
    /// Create a visual concept shape on the canvas
    /// </summary>
    string CreateConceptShape(string conceptName, string? description = null);
    
    /// <summary>
    /// Create a visual property shape on the canvas
    /// </summary>
    string CreatePropertyShape(string propertyName, string? propertyType = null);
    
    /// <summary>
    /// Create an engineering system with concept and properties
    /// </summary>
    string CreateEngineeringSystem(string systemName, params string[] properties);
    
    /// <summary>
    /// Attach a property shape to a concept shape
    /// </summary>
    bool AttachPropertyToConcept(string propertyId, string conceptId);
}

/// <summary>
/// ITechnician implementation for KnModel/KnComponent operations with context tracking
/// </summary>
[Description("Model manipulation tools for creating and managing hierarchical component models with parameters")]
public class ModelTech : IModelTech
{
    private readonly IMentorServices _mentorServices;
    private readonly IModelEditor _modelEditor;
    private IMentorStudio? _mentorStudio;
    private FoPage2D? _currentPage;
    private MentorShape2D? _lastCreatedConcept; // Track for property attachment
    
    // Use MentorServices for shared state
    public KnModel? CurrentModel 
    { 
        get => _mentorServices.CurrentModel; 
        private set => _mentorServices.CurrentModel = value;
    }
    
    public KnComponent? CurrentComponent 
    { 
        get => _mentorServices.CurrentComponent; 
        private set => _mentorServices.CurrentComponent = value;
    }

    public ModelTech(IMentorServices mentorServices)
    {
        _mentorServices = mentorServices;
        _modelEditor = new ModelEditor(mentorServices); // Create editor dynamically
        "ModelTech: Initialized".WriteSuccess();
    }

    public void ClearModel()
    {

        // Use the model's built-in method to clear all children
        CurrentModel?.ClearAllChildren();
        
        
        CurrentModel = null;
        CurrentComponent = null;
        "ModelTech: Cleared current model and component".WriteInfo();
    }
    
    /// <summary>
    /// Set the current page context and mentor studio for visual shape creation
    /// Must be called before visual shape creation methods
    /// </summary>
    public void SetPageContext(FoPage2D page, IMentorStudio mentorStudio)
    {
        _currentPage = page;
        _mentorStudio = mentorStudio;
        $"ModelTech: Connected to page '{page.GetName()}' with MentorStudio".WriteSuccess();
    }
    
    [AgentTool("establish_model")]
    [Description("Create or retrieve a named model and set as current working model")]
    public KnModel EstablishModel(
        [Description("Name of the model to create or retrieve")] string modelName, 
        [Description("Type of model (defaults to 'KnModel')")] string? modelType = null)
    {
        try
        {
            modelType ??= "PartModel";
            
            $"ModelTech.EstablishModel: Creating/retrieving '{modelName}' of type {modelType}".WriteInfo();
            
            // Support PartModel (default), KnModel, and AnimatedKnModel
            KnModel model = modelType.ToLower() switch
            {
                "animatedknmodel" => _mentorServices.EstablishModel<AnimatedKnModel>(modelName),
                "knmodel" => _mentorServices.EstablishModel<KnModel>(modelName),
                _ => _mentorServices.EstablishModel<PartModel>(modelName)
            };
            
            model.SetExpanded(true);
            CurrentModel = model;
            CurrentComponent = null; // Reset component when model changes
            
            // Publish event so UI knows model was created/established
            _mentorServices.PubSub.Publish(ModelEditChanged.Created(model));
            
            var componentCount = model.Members<KnComponent>().Count();
            $"✅ Model '{modelName}' established with {componentCount} components (now current)".WriteSuccess();
            return model;
        }
        catch (Exception ex)
        {
            $"❌ Error establishing model: {ex.Message}".WriteError();
            throw;
        }
    }

    [AgentTool("set_current_model")]
    [Description("Set the current working model by name")]
    public KnModel? SetCurrentModel([Description("Name of the model (null to clear)")] string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
        {
            CurrentModel = null;
            CurrentComponent = null;
            "⚙️ Cleared current model".WriteInfo();
            return null;
        }
        
        var model = _mentorServices.FindModel<KnModel>(modelName);
        if (model == null)
        {
            $"⚠️ Model '{modelName}' not found".WriteWarning();
            return null;
        }
        
        CurrentModel = model;
        CurrentComponent = null;
        $"⚙️ Set current model to '{modelName}'".WriteInfo();
        return model;
    }

    [Description("Set the current working component by path")]
    public KnComponent? SetCurrentComponent([Description("Path to component (null to clear)")] string? componentPath)
    {
        if (string.IsNullOrEmpty(componentPath))
        {
            CurrentComponent = null;
            "⚙️ Cleared current component".WriteInfo();
            return null;
        }
        
        if (CurrentModel == null)
        {
            "⚠️ No current model set".WriteWarning();
            return null;
        }
        
        var component = FindComponentByPath(CurrentModel, componentPath);
        if (component == null)
        {
            $"⚠️ Component '{componentPath}' not found".WriteWarning();
            return null;
        }
        
        CurrentComponent = component;
        $"⚙️ Set current component to '{componentPath}'".WriteInfo();
        return component;
    }

    [Description("List all available models in the system")]
    public List<KnModel> ListModels()
    {
        try
        {
            var models = _mentorServices.MentorModel.GetAllModels();
            $"📋 Found {models.Count} models".WriteInfo();
            return models;
        }
        catch (Exception ex)
        {
            $"❌ Error listing models: {ex.Message}".WriteError();
            return new List<KnModel>();
        }
    }
    
    [AgentTool("add_component")]
    [Description("Add a component to the current model (or specified model) and make it current")]
    public KnComponent AddComponent(
        [Description("Name for the new component")] string componentName,
        [Description("Model name (optional, uses current model)")] string? modelName = null,
        [Description("Path to parent component (optional, defaults to model root)")] string? parentComponentPath = null)
    {
        try
        {
            var model = !string.IsNullOrEmpty(modelName) 
                ? _mentorServices.FindModel<KnModel>(modelName) 
                : CurrentModel;
                
            if (model == null)
            {
                throw new Exception(modelName != null ? $"Model '{modelName}' not found" : "No current model set");
            }
            
            // Find parent component or use model as parent
            KnComponent parent = model;
            if (!string.IsNullOrEmpty(parentComponentPath))
            {
                parent = FindComponentByPath(model, parentComponentPath) 
                    ?? throw new Exception($"Parent component '{parentComponentPath}' not found");
            }
            
            // Create PartComponent (now concrete, not abstract)
            var component = new PartComponent(componentName);
            
            // Add to parent via ModelEditor (triggers events)
            _modelEditor.AddChild(parent, component);
            
            // Make this the current component
            CurrentComponent = component;
            
            $"✅ Added component '{componentName}' (now current)".WriteSuccess();
            return component;
        }
        catch (Exception ex)
        {
            $"❌ Error adding component: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Remove current component or specified component by path")]
    public bool RemoveComponent(
        [Description("Path to component (optional, uses current component)")] string? componentPath = null)
    {
        try
        {
            if (CurrentModel == null)
            {
                "⚠️ No current model set".WriteWarning();
                return false;
            }
            
            var component = !string.IsNullOrEmpty(componentPath)
                ? FindComponentByPath(CurrentModel, componentPath)
                : CurrentComponent;
                
            if (component == null)
            {
                $"⚠️ Component not found".WriteWarning();
                return false;
            }
            
            var parent = component.GetKnParent() as KnComponent ?? CurrentModel;
            _modelEditor.RemoveChild(parent, component);
            
            // Clear current component if it was the one removed
            if (CurrentComponent == component)
            {
                CurrentComponent = null;
            }
            
            $"✅ Removed component '{componentPath}'".WriteSuccess();
            return true;
        }
        catch (Exception ex)
        {
            $"❌ Error removing component '{componentPath}': {ex.Message}".WriteError();
            return false;
        }
    }
    
    [AgentTool("set_parameter")]
    [Description("Set a parameter value on the current component. CRITICAL SYNTAX: For literal values with units, use units(value, 'unit') with unit in SINGLE QUOTES. For formulas, reference other parameters with @ suffix (no quotes on parameter names).")]
    public KnParameter SetParameter(
        [Description("Name of the parameter")] string parameterName,
        [Description("Value as formula string. EXAMPLES: Literal with units: units(12, 'V') or units(100, 'Ω'). Formula: InputVoltage@ * (R2@ / (R1@ + R2@)). Simple value: '42' or '3.14'. NEVER use units(12, V) - unit MUST be quoted!")] string value,
        [Description("Path to component (optional, uses current component)")] string? componentPath = null)
    {
        try
        {
            if (CurrentModel == null)
            {
                throw new Exception("No current model set");
            }
            
            var component = !string.IsNullOrEmpty(componentPath)
                ? FindComponentByPath(CurrentModel, componentPath)
                : CurrentComponent;
                
            if (component == null)
            {
                throw new Exception(componentPath != null ? $"Component '{componentPath}' not found" : "No current component set");
            }
            
            // Set parameter via ModelEditor (triggers dependency cascade)
            var param = _modelEditor.SetParameter(component, parameterName, value);
            
            $"✅ Set parameter '{parameterName}' = '{value}'".WriteSuccess();
            return param;
        }
        catch (Exception ex)
        {
            $"❌ Error setting parameter: {ex.Message}".WriteError();
            throw;
        }
    }

    [AgentTool("get_parameter")]
    [Description("Get parameter value from current component (or specified component)")]
    public KnParameter? GetParameter(
        [Description("Name of the parameter")] string parameterName,
        [Description("Path to component (optional, uses current component)")] string? componentPath = null)
    {
        try
        {
            if (CurrentModel == null)
            {
                "⚠️ No current model set".WriteWarning();
                return null;
            }
            
            var component = !string.IsNullOrEmpty(componentPath)
                ? FindComponentByPath(CurrentModel, componentPath)
                : CurrentComponent;
                
            if (component == null)
            {
                "⚠️ Component not found".WriteWarning();
                return null;
            }
            
            var param = component.EstablishParameter(parameterName);
            if (param == null)
            {
                $"⚠️ Parameter '{parameterName}' not found".WriteWarning();
            }
            
            return param;
        }
        catch (Exception ex)
        {
            $"❌ Error getting parameter: {ex.Message}".WriteError();
            return null;
        }
    }

    [AgentTool("list_components")]
    [Description("List all components in current model (or specified model)")]
    public List<KnComponent> ListComponents(
        [Description("Model name (optional, uses current model)")] string? modelName = null)
    {
        try
        {
            var model = !string.IsNullOrEmpty(modelName)
                ? _mentorServices.FindModel<KnModel>(modelName)
                : CurrentModel;
                
            if (model == null)
            {
                "⚠️ No model specified or current".WriteWarning();
                return new List<KnComponent>();
            }
            
            var components = model.Members<KnComponent>().ToList();
            $"📋 Found {components.Count} components".WriteInfo();
            return components;
        }
        catch (Exception ex)
        {
            $"❌ Error listing components: {ex.Message}".WriteError();
            return new List<KnComponent>();
        }
    }
    
    [AgentTool("get_component")]
    [Description("Get a component by path from the current model")]
    public OPResult GetComponent(
        [Description("Path to the component (e.g., 'Specifications' or 'Resistor1')")] string componentPath)
    {
        try
        {
            if (CurrentModel == null)
            {
                "⚠️ No current model set".WriteWarning();
                return OPResult.Error("No current model set");
            }
            
            var component = FindComponentByPath(CurrentModel, componentPath);
            if (component == null)
            {
                $"⚠️ Component '{componentPath}' not found".WriteWarning();
                return OPResult.Error($"Component '{componentPath}' not found");
            }
            
            $"✅ Found component '{componentPath}'".WriteSuccess();
            return new OPResult("component", ResultStatus.Instance, component);
        }
        catch (Exception ex)
        {
            $"❌ Error getting component: {ex.Message}".WriteError();
            return OPResult.Error($"Error getting component: {ex.Message}");
        }
    }

    // Helper methods
    
    private KnComponent? FindComponentByPath(KnModel model, string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        KnComponent current = model;
        
        foreach (var part in parts)
        {
            var child = current.Members<KnComponent>().FirstOrDefault(c => c.Name == part);
            if (child == null) return null;
            current = child;
        }
        
        return current;
    }
    
    private string GetComponentPath(KnComponent component)
    {
        var parts = new List<string>();
        var current = component;
        
        while (current != null && current is not KnModel)
        {
            parts.Insert(0, current.Name ?? "");
            current = current.GetKnParent() as KnComponent;
        }
        
        return string.Join("/", parts);
    }
    

    // ============================================
    // VISUAL SHAPE CREATION METHODS
    // ============================================

    [Description("Create a visual Model shape on the canvas with engineering properties")]
    public string CreateConceptShape(
        [Description("Name/title of the model (e.g., 'Steel Beam', 'Motor')")] string conceptName,
        [Description("Optional description of the model")] string? description = null)
    {
        if (_mentorStudio == null || _currentPage == null)
        {
            throw new InvalidOperationException("ModelTech not connected to page. Call SetPageContext first.");
        }

        try
        {
            var shape = _mentorStudio.CreateShape<KnConcept>(conceptName, _currentPage);
            
            // Position in center of canvas
            var centerX = _currentPage.PageWidth.AsPixels() / 2;
            var centerY = _currentPage.PageHeight.AsPixels() / 2;
            shape.MoveTo((int)centerX, (int)centerY);
            
            // Track for property attachment
            _lastCreatedConcept = shape;
            
            var shapeId = shape.GetGlyphId();
            $"ModelTech: Created Concept shape '{conceptName}' (ID: {shapeId}) at center ({centerX}, {centerY})".WriteSuccess();
            return shapeId;
        }
        catch (Exception ex)
        {
            $"ModelTech.CreateConceptShape failed: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Create a visual Property shape on the canvas for engineering parameters")]
    public string CreatePropertyShape(
        [Description("Name of the property (e.g., 'Yield Strength', 'Flow Rate')")] string propertyName,
        [Description("Optional value or units (e.g., '350 MPa', '100 GPM')")] string? value = null)
    {
        if (_mentorStudio == null || _currentPage == null)
        {
            throw new InvalidOperationException("ModelTech not connected to page. Call SetPageContext first.");
        }

        try
        {
            var displayName = value != null ? $"{propertyName}: {value}" : propertyName;
            var shape = _mentorStudio.CreateShape<KnProperty>(displayName, _currentPage);
            
            // Position near center but offset for readability
            var centerX = _currentPage.PageWidth.AsPixels() / 2;
            var centerY = _currentPage.PageHeight.AsPixels() / 2;
            var random = new Random();
            var offsetX = random.Next(-200, 200); // Random offset around center
            var offsetY = random.Next(-150, 150);
            shape.MoveTo((int)(centerX + offsetX), (int)(centerY + offsetY));
            
            // Attach to last created concept if available
            if (_lastCreatedConcept != null)
            {
                var attached = _mentorStudio.Attach(shape, _lastCreatedConcept);
                $"ModelTech: Attached Property '{displayName}' to Concept '{_lastCreatedConcept.Text}'".WriteSuccess();
                
                // Trigger parent shape to resize and encompass children
                _lastCreatedConcept.ResizeToFitChildren();
                $"ModelTech: Resized parent concept '{_lastCreatedConcept.Text}' to encompass children".WriteSuccess();
            }
            
            var shapeId = shape.GetGlyphId();
            $"ModelTech: Created Property shape '{displayName}' (ID: {shapeId})".WriteSuccess();
            return shapeId;
        }
        catch (Exception ex)
        {
            $"ModelTech.CreatePropertyShape failed: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Create an engineering system model with components and key properties")]
    public string CreateEngineeringSystem(
        [Description("Name of the engineering system (e.g., 'Bridge Beam', 'Pump System')")] string systemName,
        [Description("Array of key properties to include (e.g., ['Length', 'Material', 'Load Capacity'])")] string[] properties)
    {
        if (_mentorStudio == null || _currentPage == null)
        {
            throw new InvalidOperationException("ModelTech not connected to page. Call SetPageContext first.");
        }

        try
        {
            // Create the main concept
            var conceptShape = _mentorStudio.CreateShape<KnConcept>(systemName, _currentPage);
            var conceptId = conceptShape.GetGlyphId();
            
            // Create and attach properties  
            foreach (var propName in properties)
            {
                var propShape = _mentorStudio.CreateShape<KnProperty>(propName, _currentPage);
                _mentorStudio.Attach(propShape, conceptShape);
            }
            
            // Trigger parent shape to resize and encompass all children
            conceptShape.ResizeToFitChildren();
            
            $"ModelTech: Created engineering system '{systemName}' with {properties.Length} properties and resized to fit".WriteSuccess();
            return conceptId;
        }
        catch (Exception ex)
        {
            $"ModelTech.CreateEngineeringSystem failed: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Attach a property shape to a model shape for visual containment")]
    public bool AttachPropertyToConcept(
        [Description("ID of the property shape to attach")] string propertyId,
        [Description("ID of the model shape to attach to")] string conceptId)
    {
        if (_mentorStudio == null || _currentPage == null)
        {
            throw new InvalidOperationException("ModelTech not connected to page. Call SetPageContext first.");
        }

        try
        {
            // Find shapes by ID using public methods
            var propertyShape = _currentPage.LookupShape2D(propertyId) as MentorShape2D;
            var conceptShape = _currentPage.LookupShape2D(conceptId) as MentorShape2D;
            
            if (propertyShape == null)
            {
                $"ModelTech: Property shape with ID '{propertyId}' not found".WriteError();
                return false;
            }
            
            if (conceptShape == null)
            {
                $"ModelTech: Concept shape with ID '{conceptId}' not found".WriteError();
                return false;
            }
            
            // Attach property to concept
            _mentorStudio.Attach(propertyShape, conceptShape);
            $"ModelTech: Successfully attached Property '{propertyShape.Text}' to Concept '{conceptShape.Text}'".WriteSuccess();
            
            // Trigger parent shape to resize and encompass children
            conceptShape.ResizeToFitChildren();
            $"ModelTech: Resized concept '{conceptShape.Text}' to encompass all children".WriteSuccess();
            
            return true;
        }
        catch (Exception ex)
        {
            $"ModelTech.AttachPropertyToConcept failed: {ex.Message}".WriteError();
            return false;
        }
    }

    [Description("Create a dynamic class hierarchy using model inheritance relationships")]
    public string CreateClassHierarchy(
        [Description("Name of the root class")] string rootClassName,
        [Description("Child class names (comma-separated)")] string childClasses = "Engine,Transmission,Suspension,Brakes",
        [Description("Properties for each class (comma-separated)")] string properties = "Type,Model,Manufacturer")
    {
        if (_mentorStudio == null || _currentPage == null)
        {
            throw new InvalidOperationException("ModelTech not connected to page. Call SetPageContext first.");
        }

        try
        {
            var childNames = childClasses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(p => p.Trim())
                                        .ToArray();
            
            var propertyNames = properties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(p => p.Trim())
                                         .ToArray();
            
            // Create the root concept
            var centerX = (int)(_currentPage.Width / 2);
            var centerY = (int)(_currentPage.Height / 3); // Higher up for hierarchy
            
            var rootConcept = _mentorStudio.CreateShape<KnConcept>(rootClassName, _currentPage);
            rootConcept.MoveTo(centerX, centerY);
            
            // Add properties to root concept
            foreach (var propName in propertyNames)
            {
                var propShape = _mentorStudio.CreateShape<KnProperty>($"{rootClassName}.{propName}", _currentPage);
                _mentorStudio.Attach(propShape, rootConcept);
            }
            
            // Create child concepts and attach them to root (inheritance)
            var childConcepts = new List<MentorShape2D>();
            foreach (var childName in childNames)
            {
                var childConcept = _mentorStudio.CreateShape<KnConcept>(childName, _currentPage);
                
                // Position children below and spread horizontally
                var offsetX = (childConcepts.Count - childNames.Length / 2.0) * 250;
                childConcept.MoveTo((int)(centerX + offsetX), centerY + 200);
                
                // Add specific properties to child
                foreach (var propName in propertyNames)
                {
                    var childProp = _mentorStudio.CreateShape<KnProperty>($"{childName}.{propName}", _currentPage);
                    _mentorStudio.Attach(childProp, childConcept);
                }
                
                // Create inheritance relationship (child concept attached to parent concept)
                _mentorStudio.Attach(childConcept, rootConcept);
                childConcepts.Add(childConcept);
                
                $"ModelTech: Created child concept '{childName}' with {propertyNames.Length} properties".WriteInfo();
            }
            
            _lastCreatedConcept = rootConcept;
            var rootId = rootConcept.GetGlyphId();
            
            $"ModelTech: Created class hierarchy '{rootClassName}' with {childNames.Length} child classes and automatic resizing".WriteSuccess();
            return rootId;
        }
        catch (Exception ex)
        {
            $"ModelTech.CreateClassHierarchy failed: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Create a role-based composition structure showing organizational relationships")]
    public string CreateRoleComposition(
        [Description("Name of the organization")] string organizationName,
        [Description("Role names (comma-separated)")] string roles = "Manager,Engineer,Technician,Analyst",
        [Description("Responsibilities for each role (comma-separated)")] string responsibilities = "Planning,Design,Implementation,Testing")
    {
        if (_mentorStudio == null || _currentPage == null)
        {
            throw new InvalidOperationException("ModelTech not connected to page. Call SetPageContext first.");
        }

        try
        {
            var roleNames = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .ToArray();
            
            var respNames = responsibilities.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(p => p.Trim())
                                          .ToArray();
            
            // Create the organization context
            var centerX = (int)(_currentPage.Width / 2);
            var centerY = (int)(_currentPage.Height / 3);
            
            var orgContext = _mentorStudio.CreateShape<KnContext>(organizationName, _currentPage);
            orgContext.MoveTo(centerX, centerY);
            
            // Create roles and attach to organization
            var createdRoles = new List<MentorShape2D>();
            foreach (var roleName in roleNames)
            {
                var roleShape = _mentorStudio.CreateShape<KnRole>(roleName, _currentPage);
                
                // Position roles in a grid below organization
                var col = createdRoles.Count % 2;
                var row = createdRoles.Count / 2;
                var offsetX = (col - 0.5) * 300;
                var offsetY = (row + 1) * 150;
                
                roleShape.MoveTo((int)(centerX + offsetX), (int)(centerY + offsetY));
                
                // Add responsibilities to each role
                foreach (var respName in respNames)
                {
                    var responsibility = _mentorStudio.CreateShape<KnProperty>($"{roleName}: {respName}", _currentPage);
                    _mentorStudio.Attach(responsibility, roleShape);
                }
                
                // Attach role to organization (composition)
                _mentorStudio.Attach(roleShape, orgContext);
                createdRoles.Add(roleShape);
                
                $"ModelTech: Created role '{roleName}' with {respNames.Length} responsibilities".WriteInfo();
            }
            
            _lastCreatedConcept = orgContext;
            var orgId = orgContext.GetGlyphId();
            
            $"ModelTech: Created role composition '{organizationName}' with {roleNames.Length} roles and automatic resizing".WriteSuccess();
            return orgId;
        }
        catch (Exception ex)
        {
            $"ModelTech.CreateRoleComposition failed: {ex.Message}".WriteError();
            throw;
        }
    }
}
