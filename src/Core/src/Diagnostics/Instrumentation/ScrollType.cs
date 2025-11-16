/// <summary>
/// Defines standard scroll type identifiers for diagnostic instrumentation.
/// </summary>
internal static class ScrollType
{
	/// <summary>
	/// Scroll operation initiated by user gesture (touch, mouse wheel, etc.).
	/// </summary>
	public const string UserGesture = "UserGesture";

	/// <summary>
	/// Scroll operation initiated programmatically via code.
	/// </summary>
	public const string Programmatic = "Programmatic";

	/// <summary>
	/// Scroll operation caused by keyboard input (arrow keys, page up/down, etc.).
	/// </summary>
	public const string Keyboard = "Keyboard";

	/// <summary>
	/// Unknown or unspecified scroll operation type.
	/// </summary>
	public const string Unknown = "Unknown";
}