namespace Microsoft.Maui
{
	/// <summary>
	/// Represents an individual command in a SwipeView.
	/// </summary>
	public interface ISwipeItem : IElement
	{
		/// <summary>
		/// Gets the string that uniquely identifies the element.
		/// </summary>
		string AutomationId { get; }

		/// <summary>
		/// Occurs when user interaction indicates that the command represented by this item should execute.
		/// </summary>
		void OnInvoked();
	}
}