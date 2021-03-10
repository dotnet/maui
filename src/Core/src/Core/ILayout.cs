using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the base properties and methods for all Layout elements.
	/// Use Layout elements to position and size child elements in .NET MAUI applications.
	/// </summary>
	public interface ILayout : IView
	{
		/// <summary>
		/// Gets the collection of children that the Layout contains.
		/// </summary>
		IReadOnlyList<IView> Children { get; }

		/// <summary>
		/// Gets the Layout Handler.
		/// </summary>
		ILayoutHandler LayoutHandler { get; }

		/// <summary>
		/// Add a child View to the Layout.
		/// </summary>
		/// <param name="child">The child View to add to the Layout.</param>
		void Add(IView child);

		/// <summary>
		/// Remove a child View from the Layout.
		/// </summary>
		/// <param name="child">The child View to remove from the Layout.</param>
		void Remove(IView child);
	}
}