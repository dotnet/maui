namespace Microsoft.Maui;

/// <summary>
/// Specifies the behavior of a SwipeView when an item is invoked.
/// </summary>
public enum SwipeMode
{
	/// <summary>Display additional context items which may be selected.</summary>
	Reveal,

	/// <summary>Immediately execute the associated command upon invocation.</summary>
	Execute
}
