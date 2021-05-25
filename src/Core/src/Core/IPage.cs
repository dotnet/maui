namespace Microsoft.Maui
{
	/// <summary>
	/// A View that occupies the entire screen.
	/// </summary>
	public interface IPage : IView
	{
		/// <summary>
		/// Gets the view that contains the content of the Page.
		/// </summary>
		public IView Content { get; }

		/// <summary>
		/// Gets the title of the Page.
		/// </summary>
		public string Title { get; }
	}
}