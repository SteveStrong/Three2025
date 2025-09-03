using BlazorThreeJS.Maths;

namespace Three2025.Apprentice.Snapping;

/// <summary>
/// Engine for executing snap constraints
/// </summary>
public static class SnapEngine
{
    /// <summary>
    /// Execute a single constraint
    /// </summary>
    public static SnapResult ExecuteConstraint(SnapConstraint constraint)
    {
        // 1. Validate constraint before execution
        var validation = constraint.Validate();
        if (!validation.IsValid)
            return SnapResult.CreateFailed($"Constraint validation failed: {validation.Reason}");
        
        // 2. Execute the constraint
        var result = constraint.Execute();
        
        // 3. If successful, register the constraint with both components
        if (result.Success)
        {
            RegisterConstraintWithComponents(constraint);
        }
        
        return result;
    }
    
    /// <summary>
    /// Execute multiple constraints in priority order
    /// </summary>
    public static List<SnapResult> ExecuteConstraints(List<SnapConstraint> constraints)
    {
        var results = new List<SnapResult>();
        
        // Sort by priority (lower values first)
        var sortedConstraints = constraints
            .Where(c => c.IsActive)
            .OrderBy(c => c.Priority)
            .ToList();
        
        foreach (var constraint in sortedConstraints)
        {
            var result = ExecuteConstraint(constraint);
            results.Add(result);
            
            // Stop on first failure if needed
            if (!result.Success)
            {
                // Could add option to continue or stop on failure
                break;
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Quick utility method: Stack componentA on top of componentB
    /// </summary>
    public static SnapResult StackOnTop(ISnappable3D bottom, ISnappable3D top)
    {
        var bottomTopFace = bottom.GetFace("Top");
        var topBottomFace = top.GetFace("Bottom");
        
        if (bottomTopFace == null || topBottomFace == null)
        {
            return SnapResult.CreateFailed("Could not find required faces for stacking");
        }
        
        var constraint = new FaceToFaceConstraint(top, topBottomFace, bottom, bottomTopFace);
        return ExecuteConstraint(constraint);
    }
    
    private static void RegisterConstraintWithComponents(SnapConstraint constraint)
    {
        // Add constraint to both components' constraint lists
        if (!constraint.ComponentA.Constraints.Contains(constraint))
            constraint.ComponentA.Constraints.Add(constraint);
            
        if (!constraint.ComponentB.Constraints.Contains(constraint))
            constraint.ComponentB.Constraints.Add(constraint);
    }
}
