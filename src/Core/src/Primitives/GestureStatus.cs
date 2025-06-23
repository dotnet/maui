namespace Microsoft.Maui;

/// <summary>
/// Enumerates possible gesture states.
/// </summary>
public enum GestureStatus
{
	/// <summary>The gesture started.</summary>
	Started = 0,

	/// <summary>The gesture is still being recognized.</summary>
	Running = 1,

	/// <summary>The gesture completed.</summary>
	Completed = 2,

	/// <summary>The gesture was canceled.</summary>
	Canceled = 3
}
