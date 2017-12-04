namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Represents the arguments passed to every VisualElement change event.
	/// </summary>
	public class VisualElementChangedEventArgs : ElementChangedEventArgs<VisualElement>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.VisualElementChangedEventArgs"/> class.
		/// </summary>
		/// <param name="oldElement">Old element.</param>
		/// <param name="newElement">New element.</param>
		public VisualElementChangedEventArgs(VisualElement oldElement, VisualElement newElement) : base(oldElement, newElement) {
		}
	}
}
