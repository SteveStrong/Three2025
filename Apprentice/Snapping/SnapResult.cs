using BlazorThreeJS.Maths;

namespace Three2025.Apprentice.Snapping;

/// <summary>
/// Result of executing a snap constraint
/// </summary>
public class SnapResult
{
    public bool Success { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public Transform3 FinalTransform { get; private set; }
    public int ConstraintsApplied { get; private set; }
    public Exception Exception { get; private set; }
    
    private SnapResult(bool success, Transform3 transform = null, int constraintsApplied = 0, string errorMessage = "", Exception exception = null)
    {
        Success = success;
        FinalTransform = transform ?? new Transform3();
        ConstraintsApplied = constraintsApplied;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    public static SnapResult CreateSuccess(Transform3 transform, int constraintsApplied)
    {
        return new SnapResult(true, transform, constraintsApplied);
    }
    
    public static SnapResult CreateFailed(string reason)
    {
        return new SnapResult(false, errorMessage: reason);
    }
    
    public static SnapResult CreateFailed(string reason, Exception exception)
    {
        return new SnapResult(false, errorMessage: reason, exception: exception);
    }
}

/// <summary>
/// Result of validating a constraint before execution
/// </summary>
public class ConstraintValidation
{
    public bool IsValid { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    
    private ConstraintValidation(bool isValid, string reason = "")
    {
        IsValid = isValid;
        Reason = reason;
    }
    
    public static ConstraintValidation Valid()
    {
        return new ConstraintValidation(true);
    }
    
    public static ConstraintValidation Invalid(string reason)
    {
        return new ConstraintValidation(false, reason);
    }
}
