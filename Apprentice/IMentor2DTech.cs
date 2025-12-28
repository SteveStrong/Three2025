using FoundryWorldsAndDrawings.Shape;
using Three2025.Models.Apprentice;

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
    BoxInfo AddBox(string name, string label, int x, int y, int width, int height, string color);
    BoxInfo AddStateBox(string name, string label, int x, int y, string color);
    BoxInfo AddDecisionBox(string name, string label, int x, int y);
    
    // Connector/Link Operations
    LinkInfo AddDirectedLink(string sourceName, string targetName, string label);
    
    // Query Operations
    BoxInfo? FindBox(string name);
    List<BoxInfo> GetAllBoxes();
    List<LinkInfo> GetAllLinks();
    
    // Modification Operations
    void MoveBox(string name, int x, int y);
    void UpdateBoxLabel(string name, string newLabel);
    void DeleteBox(string name);
}
