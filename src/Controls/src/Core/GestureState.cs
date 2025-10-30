namespace Microsoft.Maui.Controls
{
	/// <summary>Enumeration specifying the various states of a gesture.</summary>
	public enum GestureState
	{
		/// <summary>The gesture has begun and has not ended, failed, or been cancelled.</summary>
		Began,
		/// <summary>The gesture state is being updated.</summary>
		Update,
		/// <summary>The gesture has ended.</summary>
		Ended,
		/// <summary>The gesture was not recognized.</summary>
		Failed,
		/// <summary>The gesture was cancelled.</summary>
		Cancelled,
		/// <summary>The gesture is in progress and may still become recognizable.</summary>
		Possible
	}
}