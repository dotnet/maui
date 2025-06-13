namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents different levels of performance warnings.
/// Used to classify the severity of performance-related issues.
/// </summary>
public enum PerformanceWarningLevel
{
	/// <summary>
	/// Informational level indicating minor performance issues that do not require immediate attention.
	/// </summary>
	Info,

	/// <summary>
	/// Warning level signaling potential performance problems that should be reviewed.
	/// </summary>
	Warning,

	/// <summary>
	/// Critical level indicating severe performance issues that require immediate action.
	/// </summary>
	Critical
}