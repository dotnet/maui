namespace Microsoft.Maui
{
	public interface IElement
	{
		/// <summary>
		/// Gets or sets the View Handler of the Element.
		/// </summary>
		IElementHandler? Handler { get; set; }

		/// <summary>
		/// Gets the Parent of the Element.
		/// </summary>
		IElement? Parent { get; }
	}
}