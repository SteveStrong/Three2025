using System.ComponentModel;
using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Components.Pages;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Interface for model manipulation operations
/// </summary>
public interface IModelTech : ITechnician
{
    /// <summary>
    /// Create or get existing model by name
    /// </summary>
    ModelInfo EstablishModel(string modelName, string? modelType = null);
    
    /// <summary>
    /// Get information about a specific model
    /// </summary>
    ModelInfo? GetModel(string modelName);
    
    /// <summary>
    /// List all available models
    /// </summary>
    List<ModelInfo> ListModels();
    
    /// <summary>
    /// Add a component to a model
    /// </summary>
    ComponentInfo AddComponent(string modelName, string componentName, string componentType, string? parentComponentPath = null);
    
    /// <summary>
    /// Remove a component from a model
    /// </summary>
    bool RemoveComponent(string modelName, string componentPath);
    
    /// <summary>
    /// Set a parameter value on a component
    /// </summary>
    ParameterInfo SetParameter(string modelName, string componentPath, string parameterName, string value);
    
    /// <summary>
    /// Get parameter value from a component
    /// </summary>
    ParameterInfo? GetParameter(string modelName, string componentPath, string parameterName);
    
    /// <summary>
    /// List all components in a model
    /// </summary>
    List<ComponentInfo> ListComponents(string modelName);
    
    /// <summary>
    /// Get component details including parameters
    /// </summary>
    ComponentInfo? GetComponent(string modelName, string componentPath);
}

/// <summary>
/// Information about a KnModel
/// </summary>
public class ModelInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int ComponentCount { get; set; }
    public int ParameterCount { get; set; }
    public bool IsExpanded { get; set; }
}

/// <summary>
/// Information about a KnComponent
/// </summary>
public class ComponentInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Type { get; set; } = "";
    public string? ParentPath { get; set; }
    public int ChildCount { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new();
}

/// <summary>
/// Information about a KnParameter
/// </summary>
public class ParameterInfo
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Unit { get; set; }
    public string? Formula { get; set; }
}

/// <summary>
/// ITechnician implementation for KnModel/KnComponent operations
/// </summary>
[Description("Model manipulation tools for creating and managing hierarchical component models with parameters")]
public class ModelTech : IModelTech
{
    private readonly IMentorServices _mentorServices;
    private readonly IModelEditor _modelEditor;

    public ModelTech(IMentorServices mentorServices, IModelEditor modelEditor)
    {
        _mentorServices = mentorServices;
        _modelEditor = modelEditor;
        "ModelTech: Initialized".WriteSuccess();
    }

    [Description("Create or get existing model by name. Returns model information including component count.")]
    public ModelInfo EstablishModel(
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
            
            var info = new ModelInfo
            {
                Name = model.Name ?? modelName,
                Type = model.GetType().Name,
                ComponentCount = model.Members<KnComponent>().Count(),
                ParameterCount = model.Members<KnParameter>().Count(),
                IsExpanded = true
            };
            
            $"✅ Model '{modelName}' established with {info.ComponentCount} components".WriteSuccess();
            return info;
        }
        catch (Exception ex)
        {
            $"❌ Error establishing model: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Get information about a specific model including component and parameter counts")]
    public ModelInfo? GetModel([Description("Name of the model")] string modelName)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null)
            {
                $"⚠️ Model '{modelName}' not found".WriteWarning();
                return null;
            }
            
            return new ModelInfo
            {
                Name = model.Name ?? modelName,
                Type = model.GetType().Name,
                ComponentCount = model.Members<KnComponent>().Count(),
                ParameterCount = model.Members<KnParameter>().Count(),
                IsExpanded = true
            };
        }
        catch (Exception ex)
        {
            $"❌ Error getting model: {ex.Message}".WriteError();
            return null;
        }
    }

    [Description("List all available models in the system")]
    public List<ModelInfo> ListModels()
    {
        try
        {
            var models = _mentorServices.MentorModel.GetAllModels();
            var infos = models.Select(m => new ModelInfo
            {
                Name = m.Name ?? "",
                Type = m.GetType().Name,
                ComponentCount = m.Members<KnComponent>().Count(),
                ParameterCount = m.Members<KnParameter>().Count(),
                IsExpanded = true
            }).ToList();
            
            $"📋 Found {infos.Count} models".WriteInfo();
            return infos;
        }
        catch (Exception ex)
        {
            $"❌ Error listing models: {ex.Message}".WriteError();
            return new List<ModelInfo>();
        }
    }

    [Description("Add a component to a model. Component path uses '/' separator (e.g., 'Root/SubSystem')")]
    public ComponentInfo AddComponent(
        [Description("Name of the model")] string modelName,
        [Description("Name for the new component")] string componentName,
        [Description("Type of component (KnComponent, PartComponent, etc.)")] string componentType,
        [Description("Path to parent component (optional, defaults to model root)")] string? parentComponentPath = null)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null)
            {
                throw new Exception($"Model '{modelName}' not found");
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
            
            var info = new ComponentInfo
            {
                Name = component.Name ?? componentName,
                Path = GetComponentPath(component),
                Type = component.GetType().Name,
                ParentPath = parent != model ? GetComponentPath(parent) : null,
                ChildCount = component.Members<KnComponent>().Count(),
                Parameters = GetComponentParameters(component)
            };
            
            $"✅ Added component '{componentName}' to '{modelName}'".WriteSuccess();
            return info;
        }
        catch (Exception ex)
        {
            $"❌ Error adding component: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Remove a component from a model using its path")]
    public bool RemoveComponent(
        [Description("Name of the model")] string modelName,
        [Description("Path to component to remove")] string componentPath)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null)
            {
                $"⚠️ Model '{modelName}' not found".WriteWarning();
                return false;
            }
            
            var component = FindComponentByPath(model, componentPath);
            if (component == null)
            {
                $"⚠️ Component '{componentPath}' not found".WriteWarning();
                return false;
            }
            
            var parent = component.GetKnParent() as KnComponent ?? model;
            _modelEditor.RemoveChild(parent, component);
            
            $"✅ Removed component '{componentPath}' from '{modelName}'".WriteSuccess();
            return true;
        }
        catch (Exception ex)
        {
            $"❌ Error removing component: {ex.Message}".WriteError();
            return false;
        }
    }

    [Description("Set a parameter value on a component. Value can be a number, formula, or units expression")]
    public ParameterInfo SetParameter(
        [Description("Name of the model")] string modelName,
        [Description("Path to component")] string componentPath,
        [Description("Name of the parameter")] string parameterName,
        [Description("Value as formula string (e.g., '42', 'Width * 2', 'units(100, \"cm\")')")] string value)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null)
            {
                throw new Exception($"Model '{modelName}' not found");
            }
            
            var component = FindComponentByPath(model, componentPath);
            if (component == null)
            {
                throw new Exception($"Component '{componentPath}' not found");
            }
            
            // Set parameter via ModelEditor (triggers dependency cascade)
            var param = _modelEditor.SetParameter(component, parameterName, value);
            
            var info = new ParameterInfo
            {
                Name = param.Name ?? parameterName,
                Value = param.GetValue()?.ToString() ?? "",
                Unit = null,
                Formula = param.Formula?.ToString() ?? ""
            };
            
            $"✅ Set parameter '{parameterName}' = '{value}' on '{componentPath}'".WriteSuccess();
            return info;
        }
        catch (Exception ex)
        {
            $"❌ Error setting parameter: {ex.Message}".WriteError();
            throw;
        }
    }

    [Description("Get parameter value from a component")]
    public ParameterInfo? GetParameter(
        [Description("Name of the model")] string modelName,
        [Description("Path to component")] string componentPath,
        [Description("Name of the parameter")] string parameterName)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null) return null;
            
            var component = FindComponentByPath(model, componentPath);
            if (component == null) return null;
            
            var param = component.EstablishParameter(parameterName);
            if (param == null) return null;
            
            return new ParameterInfo
            {
                Name = param.Name ?? parameterName,
                Value = param.GetValue()?.ToString() ?? "",
                Unit = null,
                Formula = param.Formula?.ToString() ?? ""
            };
        }
        catch (Exception ex)
        {
            $"❌ Error getting parameter: {ex.Message}".WriteError();
            return null;
        }
    }

    [Description("List all components in a model with their paths and types")]
    public List<ComponentInfo> ListComponents([Description("Name of the model")] string modelName)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null)
            {
                $"⚠️ Model '{modelName}' not found".WriteWarning();
                return new List<ComponentInfo>();
            }
            
            var components = new List<ComponentInfo>();
            CollectComponents(model, components);
            
            $"📋 Found {components.Count} components in '{modelName}'".WriteInfo();
            return components;
        }
        catch (Exception ex)
        {
            $"❌ Error listing components: {ex.Message}".WriteError();
            return new List<ComponentInfo>();
        }
    }

    [Description("Get detailed information about a specific component including all parameters")]
    public ComponentInfo? GetComponent(
        [Description("Name of the model")] string modelName,
        [Description("Path to component")] string componentPath)
    {
        try
        {
            var model = _mentorServices.FindModel<KnModel>(modelName);
            if (model == null) return null;
            
            var component = FindComponentByPath(model, componentPath);
            if (component == null) return null;
            
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
            Formula = p.Formula?.ToString() ?? ""
        }).ToList();
    }
}
