using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the base properties and methods for all Layout elements.
	/// Use Layout elements to position and size child elements in .NET MAUI applications.
	/// </summary>
	public interface ILayout : IView, IContainer
	{
		/// <summary>
		/// Gets the Layout Handler.
		/// </summary>
		ILayoutHandler LayoutHandler { get; }

		/// <summary>
		/// The space between the outer edge of the ILayout's content area and its children.
		/// </summary>
		Thickness Padding { get; }
	}
}