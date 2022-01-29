using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a collection of SwipeItem objects.
	/// </summary>
	public interface ISwipeItems : IList<ISwipeItem>
	{
		/// <summary>
		/// Gets a value that indicates the effect of a swipe interaction.
		/// </summary>
		public SwipeMode Mode { get; }

		/// <summary>
		/// Defines constants that specify how a SwipeView behaves after a command is invoked.
		/// </summary>
		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked { get; }
	}
}