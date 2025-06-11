namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents statistical data related to image loading performance.
/// </summary>
public class ImageStats
{
	/// <summary>
	/// Stores the duration (in milliseconds) taken to load an image.
	/// </summary>
	/// <remarks>
	/// This value is used for performance monitoring and optimization.
	/// It helps analyze the efficiency of image rendering within the application.
	/// </remarks>
	public double LoadDuration { get; set; }
}