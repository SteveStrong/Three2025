using FoundryWorldsAndDrawings.Shape;
using FoundryMentorModeler.Evaluator;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Interface for Mentor 2D technician - creates and manages mentor-based diagram shapes
/// </summary>
public interface IMentor2DTech
{
    // Canvas Management
    FoPage2D EstablishCanvas2D(string? pageName = null);
    void SetPage(FoPage2D page);
    
    // Box/Node Operations
    OPResult AddBox(string name, string label, int x, int y, int width, int height, string color);
    OPResult AddStateBox(string name, string label, int x, int y, string color);
    OPResult AddDecisionBox(string name, string label, int x, int y);
    
    // Connector/Link Operations
    OPResult AddDirectedLink(string sourceName, string targetName, string label);
    
    // Query Operations
    OPResult FindBox(string name);
    OPResult GetAllBoxes();
    OPResult GetAllLinks();
    
    // Modification Operations
    void MoveBox(string name, int x, int y);
    void UpdateBoxLabel(string name, string newLabel);
    void DeleteBox(string name);
}
