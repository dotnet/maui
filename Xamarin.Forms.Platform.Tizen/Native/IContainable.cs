using System.Collections.Generic;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Interface defining methods for managing elements of the container.
	/// </summary>
	/// <typeparam name="T">The type of element that can be added to the container.</typeparam>
	public interface IContainable<T>
	{
		/// <summary>
		/// The children collection of an element.
		/// </summary>
		IList<T> Children { get; }
	}
}
