namespace Microsoft.Maui
{
	/// <summary>
	/// Represent the title content used in Navigation Views.
	/// </summary>
	public interface ITitledElement : IElement
	{
		/// <summary>
		/// Gets the title of this element.
		/// </summary>
		string? Title { get; }
	}
}