using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using Three2025.Models.Apprentice;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Technician for creating and managing 2D diagram shapes with AI-discoverable tools
/// Uses FoShape2D with styling to create diagram-like boxes and connectors
/// </summary>
public class Mentor2DTech : IMentor2DTech
{
    private readonly IWorkspace _workspace;
    private readonly IFoundryService _foundryService;
    private readonly IMentor2DEditor _editor;
    private FoPage2D? _page;
    private readonly ILogger<Mentor2DTech> _logger;
    
    // Knowledge-aware dependencies (optional - injected when available)
    private readonly IMentorStudio? _studio;
    private readonly IMentorModelManager? _modelManager;
    
    // Track shapes and actions for learning
    private readonly Dictionary<string, MentorShape2D> _knowledgeShapes = new();
    private readonly List<HumanAction> _actionHistory = new(1000); // Keep last 1000 actions

    public Mentor2DTech(
        IWorkspace workspace, 
        IFoundryService foundryService, 
        ILogger<Mentor2DTech> logger,
        IMentorStudio? studio = null,
        IMentorModelManager? modelManager = null)
    {
        _workspace = workspace;
        _foundryService = foundryService;
        _editor = new Mentor2DEditor(foundryService); // Create editor dynamically
        _logger = logger;
        _studio = studio;
        _modelManager = modelManager;
    }

    public FoPage2D EstablishCanvas2D(string? pageName = null)
    {
        try
        {
            var drawing = _workspace.GetDrawing();
            
            if (!string.IsNullOrEmpty(pageName))
            {
                _page = drawing.EstablishPage<FoPage2D>(pageName);
                _logger.LogInformation("Mentor2DTech: Connected to page '{PageName}'", pageName);
            }
            else
            {
                _page = drawing.FirstPage();
                _logger.LogInformation("Mentor2DTech: Using first page '{PageName}'", _page.Name);
            }
            
            _editor.SetPage(_page); // Connect editor to page
            return _page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish Canvas2D");
            throw;
        }
    }

    public void SetPage(FoPage2D page)
    {
        _page = page;
        _editor.SetPage(page); // Connect editor to page
    }

    private FoPage2D GetPage()
    {
        if (_page == null)
        {
            EstablishCanvas2D("AgentCanvas2D");
        }
        return _page!;
    }

    // ============================================
    // BOX/NODE OPERATIONS
    // ============================================

    [Description("Add a standard rectangular box to the diagram with label and position")]
    public OPResult AddBox(
        [Description("Unique name for the box")] string name,
        [Description("Display label text")] string label,
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y,
        [Description("Width in pixels")] int width,
        [Description("Height in pixels")] int height,
        [Description("Box color (red, blue, green) or hex code (#ff0000)")] string color)
    {
        try
        {
            $"Mentor2DTech.AddBox: {name}, '{label}' at ({x},{y}) size {width}x{height}, {color}".WriteInfo();

            GetPage(); // Ensure page is established
            
            var box = _editor.AddBox(name, x, y, width, height, color);
            return new OPResult("box", ResultStatus.Shape2D, box);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add box '{Name}'", name);
            return OPResult.Error($"Failed to add box '{name}': {ex.Message}");
        }
    }

    [Description("Add a state box with rounded corners for state diagrams")]
    public OPResult AddStateBox(
        [Description("Unique name for the state")] string name,
        [Description("State label text")] string label,
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y,
        [Description("State color (red, blue, green) or hex code")] string color)
    {
        try
        {
            $"Mentor2DTech.AddStateBox: {name}, '{label}' at ({x},{y}), {color}".WriteInfo();

            GetPage(); // Ensure page is established
            
            var box = _editor.AddStateBox(name, x, y, color);
            return new OPResult("stateBox", ResultStatus.Shape2D, box);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add state box '{Name}'", name);
            return OPResult.Error($"Failed to add state box '{name}': {ex.Message}");
        }
    }

    [Description("Add a diamond-shaped decision box for flowcharts")]
    public OPResult AddDecisionBox(
        [Description("Unique name for the decision")] string name,
        [Description("Decision question text")] string label,
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y)
    {
        try
        {
            $"Mentor2DTech.AddDecisionBox: {name}, '{label}' at ({x},{y})".WriteInfo();

            GetPage(); // Ensure page is established
            
            var box = _editor.AddDecisionBox(name, x, y);
            return new OPResult("decisionBox", ResultStatus.Shape2D, box);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add decision box '{Name}'", name);
            return OPResult.Error($"Failed to add decision box '{name}': {ex.Message}");
        }
    }

    // ============================================
    // LINK OPERATIONS
    // ============================================

    [Description("Add a directed link (arrow) connecting two boxes")]
    public OPResult AddDirectedLink(
        [Description("Name of the source box")] string sourceName,
        [Description("Name of the target box")] string targetName,
        [Description("Optional label for the link")] string label)
    {
        try
        {
            $"Mentor2DTech.AddDirectedLink: {sourceName} -> {targetName}, label='{label}'".WriteInfo();

            GetPage(); // Ensure page is established
            
            var link = _editor.AddDirectedLink(sourceName, targetName);
            return new OPResult("link", ResultStatus.Shape1D, link);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add link from '{Source}' to '{Target}'", sourceName, targetName);
            return OPResult.Error($"Failed to add link from '{sourceName}' to '{targetName}': {ex.Message}");
        }
    }

    // ============================================
    // QUERY OPERATIONS
    // ============================================

    [Description("Find a box by name and return its information")]
    public OPResult FindBox([Description("Name of the box to find")] string name)
    {
        var box = _editor.FindBox(name);
        if (box != null)
        {
            return new OPResult("box", ResultStatus.Shape2D, box);
        }
        return OPResult.Error($"Box '{name}' not found");
    }

    [Description("Get a list of all boxes in the diagram")]
    public OPResult GetAllBoxes()
    {
        var boxes = _editor.GetAllBoxes();
        return new OPResult("boxes", ResultStatus.Collection, boxes);
    }

    [Description("Get a list of all links in the diagram")]
    public OPResult GetAllLinks()
    {
        var links = _editor.GetAllLinks();
        return new OPResult("links", ResultStatus.Collection, links);
    }

    // ============================================
    // MODIFICATION OPERATIONS
    // ============================================

    [Description("Move a box to a new position")]
    public void MoveBox(
        [Description("Name of the box to move")] string name,
        [Description("New X coordinate")] int x,
        [Description("New Y coordinate")] int y)
    {
        try
        {
            var box = _editor.FindBox(name);
            if (box == null)
            {
                throw new ArgumentException($"Box '{name}' not found");
            }

            box.MoveTo(x, y);
            $"Mentor2DTech.MoveBox: {name} moved to ({x},{y})".WriteInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move box '{Name}'", name);
            throw;
        }
    }

    [Description("Update the label text of a box")]
    public void UpdateBoxLabel(
        [Description("Name of the box")] string name,
        [Description("New label text")] string newLabel)
    {
        try
        {
            var box = _editor.FindBox(name);
            if (box == null)
            {
                throw new ArgumentException($"Box '{name}' not found");
            }

            // TODO: Labels require FoText2D overlay shapes
            box.Name = newLabel;  // Update name as approximation
            $"Mentor2DTech.UpdateBoxLabel: {name} label updated to '{newLabel}'".WriteInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update box label '{Name}'", name);
            throw;
        }
    }

    [Description("Delete a box from the diagram")]
    public void DeleteBox([Description("Name of the box to delete")] string name)
    {
        try
        {
            var box = _editor.FindBox(name);
            if (box == null)
            {
                throw new ArgumentException($"Box '{name}' not found");
            }

            var page = GetPage();
            page.RemoveShape(box);
            // Editor handles cleanup internally via Clear() if needed
            
            // Remove any links connected to this box (simplified for now)
            var linksToRemove = new List<string>();
            // TODO: Track source/target relationships for proper link cleanup
            
            foreach (var linkName in linksToRemove)
            {
                var link = _editor.FindLink(linkName);
                if (link != null)
                {
                    page.RemoveShape(link);
                }
            }

            $"Mentor2DTech.DeleteBox: {name} deleted with {linksToRemove.Count} connected links".WriteInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete box '{Name}'", name);
            throw;
        }
    }

    // ============================================
    // KNOWLEDGE-AWARE OPERATIONS (Conversational Modeling)
    // ============================================

    [Description("Create a knowledge shape (Concept, Property, Role, Context, Component, etc.) on the canvas")]
    public OPResult CreateKnowledgeShape(
        [Description("Type: Concept, Property, Role, Context, Component, Feature, Formula, Variable, ValidValues")]
        string knowledgeType,
        [Description("Display title/label")] 
        string title,
        [Description("X position in pixels")] 
        int x,
        [Description("Y position in pixels")] 
        int y)
    {
        if (_studio == null)
        {
            return OPResult.Error("Knowledge modeling requires IMentorStudio to be injected");
        }

        try
        {
            $"Mentor2DTech.CreateKnowledgeShape: {knowledgeType} '{title}' at ({x},{y})".WriteInfo();

            // Parse knowledge type enum
            var type = Enum.Parse<KnowledgeType>(knowledgeType, ignoreCase: true);
            
            // Delegate to studio (uses MentorStudio.CreateShape pattern)
            var shape = type switch
            {
                KnowledgeType.Concept => _studio.CreateShape<KnConcept>(title, GetPage()),
                KnowledgeType.Property => _studio.CreateShape<KnProperty>(title, GetPage()),
                KnowledgeType.Role => _studio.CreateShape<KnRole>(title, GetPage()),
                KnowledgeType.Context => _studio.CreateShape<KnContext>(title, GetPage()),
                KnowledgeType.Component => _studio.CreateShape<KnComponent>(title, GetPage()),
                KnowledgeType.Feature => _studio.CreateShape<KnFeature>(title, GetPage()),
                KnowledgeType.Formula => _studio.CreateShape<KnFormula>(title, GetPage()),
                KnowledgeType.Variable => _studio.CreateShape<KnVariable>(title, GetPage()),
                KnowledgeType.ValidValues => _studio.CreateShape<KnValidValues>(title, GetPage()),
                KnowledgeType.Relation => _studio.CreateShape<KnRelation>(title, GetPage()),
                KnowledgeType.Resource => _studio.CreateShape<KnResource>(title, GetPage()),
                KnowledgeType.Trait => _studio.CreateShape<KnTrait>(title, GetPage()),
                KnowledgeType.DefaultValue => _studio.CreateShape<KnDefaultValue>(title, GetPage()),
                _ => throw new ArgumentException($"Unknown knowledge type: {knowledgeType}")
            };

            // Position the shape
            shape.MoveTo(x, y);
            
            // Track it
            _knowledgeShapes[title] = shape;
            
            // Log action for learning
            LogAction("CreateShape", knowledgeType, new Dictionary<string, string>
            {
                ["title"] = title,
                ["x"] = x.ToString(),
                ["y"] = y.ToString(),
                ["shapeId"] = shape.GlyphId
            });
            
            // Return the actual shape object
            return new OPResult("knowledgeShape", ResultStatus.Shape2D, shape);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create knowledge shape '{Type}' '{Title}'", knowledgeType, title);
            return OPResult.Error($"Failed to create knowledge shape '{knowledgeType}' '{title}': {ex.Message}");
        }
    }

    [Description("Attach one shape to another - system determines containment vs connection")]
    public OPResult AttachShape(
        [Description("Name of child/source shape")]
        string childName,
        [Description("Name of parent/target shape")]
        string parentName)
    {
        if (_studio == null)
        {
            return OPResult.Error("Knowledge modeling requires IMentorStudio to be injected");
        }

        try
        {
            if (!_knowledgeShapes.ContainsKey(childName))
            {
                return OPResult.Error($"Shape '{childName}' not found");
            }
            if (!_knowledgeShapes.ContainsKey(parentName))
            {
                return OPResult.Error($"Shape '{parentName}' not found");
            }

            var child = _knowledgeShapes[childName];
            var parent = _knowledgeShapes[parentName];
            
            $"Mentor2DTech.AttachShape: {childName} -> {parentName}".WriteInfo();

            // Check what kind of attachment is allowed
            var isDropAllowed = parent.IsDropAllowed(child);
            var isConnectAllowed = parent.IsConnectAllowed(child);

            if (!isDropAllowed && !isConnectAllowed)
            {
                return OPResult.Error($"Cannot attach {child.GetKnowledgeType()} to {parent.GetKnowledgeType()}");
            }

            // Delegate to studio.Attach() - it handles both containment and connection
            var result = _studio.Attach(child, parent);
            
            // Determine what happened and log
            if (isConnectAllowed)
            {
                var connectorId = result.UpstreamShape?.GetGlyphId();
                
                // Log action
                LogAction("ConnectShape", $"{child.GetKnowledgeType()}->{parent.GetKnowledgeType()}", 
                    new Dictionary<string, string>
                    {
                        ["childName"] = childName,
                        ["parentName"] = parentName,
                        ["connectorId"] = connectorId ?? ""
                    });
                    
                // Return the connector shape if connection
                if (result.UpstreamShape != null)
                {
                    return new OPResult("connector", ResultStatus.Shape1D, result.UpstreamShape);
                }
            }
            else
            {
                // Log action
                LogAction("AttachShape", $"{child.GetKnowledgeType()}->{parent.GetKnowledgeType()}", 
                    new Dictionary<string, string>
                    {
                        ["childName"] = childName,
                        ["parentName"] = parentName
                    });
            }

            return OPResult.Success($"Attached '{childName}' to '{parentName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach '{Child}' to '{Parent}'", childName, parentName);
            return OPResult.Error($"Failed to attach '{childName}' to '{parentName}': {ex.Message}");
        }
    }

    [Description("Check if one shape can be attached to another")]
    public bool CanAttach(
        [Description("Name of child shape")]
        string childName,
        [Description("Name of parent shape")]
        string parentName)
    {
        if (!_knowledgeShapes.ContainsKey(childName) || !_knowledgeShapes.ContainsKey(parentName))
        {
            return false;
        }

        var child = _knowledgeShapes[childName];
        var parent = _knowledgeShapes[parentName];
        
        return parent.IsDropAllowed(child) || parent.IsConnectAllowed(child);
    }

    [Description("Get list of knowledge types that can be attached to a shape")]
    public List<string> GetAllowedChildTypes(
        [Description("Name of the parent shape")]
        string shapeName)
    {
        if (!_knowledgeShapes.ContainsKey(shapeName))
        {
            return new List<string>();
        }

        var parent = _knowledgeShapes[shapeName];
        var parentType = parent.GetKnowledgeType();
        
        // Query all knowledge types to see which can be dropped/connected
        var allowedTypes = new List<string>();
        var allTypes = Enum.GetValues<KnowledgeType>();
        
        foreach (var childType in allTypes)
        {
            // This is a simplified check - would need actual shape instances to test properly
            // For now, return common patterns
            var pattern = (childType, parentType);
            if (IsCommonPattern(pattern))
            {
                allowedTypes.Add(childType.ToString());
            }
        }
        
        return allowedTypes;
    }

    // ============================================
    // LEARNING / OBSERVATION METHODS
    // ============================================

    [Description("Get recent human actions (last N operations)")]
    public List<HumanAction> GetRecentActions(
        [Description("Number of recent actions to retrieve")]
        int count = 10)
    {
        return _actionHistory.TakeLast(count).ToList();
    }

    [Description("Get statistics about construction patterns")]
    public ConstructionStats GetConstructionPatterns()
    {
        var shapeFrequency = new Dictionary<string, int>();
        var containmentPatterns = new Dictionary<string, List<string>>();
        var connectionPatterns = new Dictionary<string, List<string>>();
        var sequences = new List<string>();

        // Analyze action history
        foreach (var action in _actionHistory)
        {
            // Count shape type frequency
            if (action.ActionType == "CreateShape")
            {
                var type = action.KnowledgeType;
                shapeFrequency[type] = shapeFrequency.GetValueOrDefault(type, 0) + 1;
            }

            // Track containment patterns
            if (action.ActionType == "AttachShape" && action.Details.ContainsKey("childName") && action.Details.ContainsKey("parentName"))
            {
                var pattern = action.KnowledgeType; // Format: "ChildType->ParentType"
                var parts = pattern.Split("->");
                if (parts.Length == 2)
                {
                    var parentType = parts[1];
                    var childType = parts[0];
                    
                    if (!containmentPatterns.ContainsKey(parentType))
                    {
                        containmentPatterns[parentType] = new List<string>();
                    }
                    if (!containmentPatterns[parentType].Contains(childType))
                    {
                        containmentPatterns[parentType].Add(childType);
                    }
                }
            }

            // Track connection patterns
            if (action.ActionType == "ConnectShape")
            {
                var pattern = action.KnowledgeType; // Format: "SourceType->TargetType"
                var parts = pattern.Split("->");
                if (parts.Length == 2)
                {
                    var sourceType = parts[0];
                    var targetType = parts[1];
                    
                    if (!connectionPatterns.ContainsKey(sourceType))
                    {
                        connectionPatterns[sourceType] = new List<string>();
                    }
                    if (!connectionPatterns[sourceType].Contains(targetType))
                    {
                        connectionPatterns[sourceType].Add(targetType);
                    }
                }
            }
        }

        // Extract frequent sequences (simplified - just last 5 action types)
        sequences = _actionHistory
            .TakeLast(5)
            .Select(a => $"{a.ActionType}({a.KnowledgeType})")
            .ToList();

        return new ConstructionStats(
            shapeFrequency,
            containmentPatterns,
            connectionPatterns,
            sequences
        );
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private void LogAction(string actionType, string knowledgeType, Dictionary<string, string> details)
    {
        var action = new HumanAction(
            DateTime.Now,
            actionType,
            knowledgeType,
            details
        );
        
        _actionHistory.Add(action);
        
        // Keep list size manageable
        if (_actionHistory.Count > 1000)
        {
            _actionHistory.RemoveAt(0);
        }
    }

    private bool IsCommonPattern((KnowledgeType child, KnowledgeType parent) pattern)
    {
        // Common containment patterns
        return pattern switch
        {
            (KnowledgeType.Property, KnowledgeType.Context) => true,
            (KnowledgeType.Property, KnowledgeType.Concept) => true,
            (KnowledgeType.Property, KnowledgeType.Relation) => true,
            (KnowledgeType.Property, KnowledgeType.Component) => true,
            (KnowledgeType.Concept, KnowledgeType.Role) => true,
            (KnowledgeType.Concept, KnowledgeType.Feature) => true,
            (KnowledgeType.Variable, KnowledgeType.Concept) => true,
            (KnowledgeType.Variable, KnowledgeType.Component) => true,
            (KnowledgeType.Formula, KnowledgeType.Role) => true,
            (KnowledgeType.Trait, KnowledgeType.Concept) => true,
            (KnowledgeType.ValidValues, KnowledgeType.Property) => true,
            (KnowledgeType.DefaultValue, KnowledgeType.Context) => true,
            _ => false
        };
    }
}
