namespace Microsoft.Maui;

/// <summary>
/// Specifies the behavior of a SwipeView when an item is invoked.
/// </summary>
public enum SwipeBehaviorOnInvoked
{
	/// <summary>In Reveal mode, the SwipeView closes after invocation; in Execute mode, remains open.</summary>
	Auto,

	/// <summary>The SwipeView closes after an item is invoked.</summary>
	Close,

	/// <summary>The SwipeView remains open after an item is invoked.</summary>
	RemainOpen
}
