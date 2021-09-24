namespace Microsoft.Maui
{
	public interface ITitledElement : IElement
	{
		/// <summary>
		/// Gets the title of this element.
		/// </summary>
		string? Title { get; }
	}
}