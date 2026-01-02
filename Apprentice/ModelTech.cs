using System.ComponentModel;
using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Components.Pages;
using Three2025.Models.Apprentice;

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
}

/// <summary>
/// ITechnician implementation for KnModel/KnComponent operations with context tracking
/// </summary>
[Description("Model manipulation tools for creating and managing hierarchical component models with parameters")]
public class ModelTech : IModelTech
{
    private readonly IMentorServices _mentorServices;
    private readonly IModelEditor _modelEditor;
    
    public KnModel? CurrentModel { get; private set; }
    public KnComponent? CurrentComponent { get; private set; }

    public ModelTech(IMentorServices mentorServices, IModelEditor modelEditor)
    {
        _mentorServices = mentorServices;
        _modelEditor = modelEditor;
        "ModelTech: Initialized".WriteSuccess();
    }
    
    [Description("Create or retrieve a named model and set as current working model")]
    public KnModel EstablishModel(
        [Description("Name of the model to create or retrieve")] string modelName, 
        [Description("Type of model (defaults to 'KnModel')")] string? modelType = null)
    {
        try
        {
            modelType ??= "KnModel";
            
            $"ModelTech.EstablishModel: Creating/retrieving '{modelName}' of type {modelType}".WriteInfo();
            
            // For now, we only support KnModel and AnimatedKnModel
            KnModel model = modelType.ToLower() switch
            {
                "animatedknmodel" => _mentorServices.EstablishModel<AnimatedKnModel>(modelName),
                _ => _mentorServices.EstablishModel<KnModel>(modelName)
            };
            
            model.SetExpanded(true);
            CurrentModel = model;
            CurrentComponent = null; // Reset component when model changes
            
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
            
            // Create component - for now only support KnComponent
            var component = new KnComponent(componentName);
            
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
    
    [Description("Set a parameter value on the current component (or specified component). Value can be a number, formula, or units expression")]
    public KnParameter SetParameter(
        [Description("Name of the parameter")] string parameterName,
        [Description("Value as formula string (e.g., '42', 'Width * 2', 'units(100, \"cm\")')")] string value,
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
    
    public ComponentInfo? GetComponent(string componentPath)
    {
        try
        {
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
            
            var parent = component.GetKnParent() as KnComponent;
            
            return new ComponentInfo
            {
                Name = component.Name ?? "",
                Path = GetComponentPath(component),
                Type = component.GetType().Name,
                ParentPath = parent != null ? GetComponentPath(parent) : null,
                ChildCount = component.Members<KnComponent>().Count(),
                Parameters = GetComponentParameters(component)
            };
        }
        catch (Exception ex)
        {
            $"❌ Error getting component: {ex.Message}".WriteError();
            return null;
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
    
    private void CollectComponents(KnComponent parent, List<ComponentInfo> infos)
    {
        foreach (var child in parent.Members<KnComponent>())
        {
            var parentComp = child.GetKnParent() as KnComponent;
            
            infos.Add(new ComponentInfo
            {
                Name = child.Name ?? "",
                Path = GetComponentPath(child),
                Type = child.GetType().Name,
                ParentPath = parentComp != null && parentComp is not KnModel ? GetComponentPath(parentComp) : null,
                ChildCount = child.Members<KnComponent>().Count(),
                Parameters = GetComponentParameters(child)
            });
            
            // Recurse into children
            CollectComponents(child, infos);
        }
    }
    
    private List<ParameterInfo> GetComponentParameters(KnComponent component)
    {
        return component.Members<KnParameter>().Select(p => new ParameterInfo
        {
            Name = p.Name ?? "",
            Value = p.GetValue()?.ToString() ?? "",
            Unit = null,
            IsFormula = !string.IsNullOrEmpty(p.Expression)
        }).ToList();
    }
}
