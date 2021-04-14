
namespace Microsoft.Maui
{
	/// <summary>
	/// A View capable of scrolling if its Content requires.
	/// </summary>
	public interface IScroll : IView
	{
		/// <summary>
		/// Gets the content of the scroll.
		/// </summary>
		IView Content { get; }
	}
}