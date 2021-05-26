using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IContainer
	{
		/// <summary>
		/// Gets the collection of children that the Container contains.
		/// </summary>
		IReadOnlyList<IView> Children { get; }
	}
}
