using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to act as containers for views. 
	/// </summary>
	public interface IContainer
	{
		/// <summary>
		/// Gets the collection of children that the Container contains.
		/// </summary>
		IReadOnlyList<IView> Children { get; }
	}
}
