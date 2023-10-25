using System.Collections.Generic;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Extension class, provides native embedding functionalities.
	/// </summary>
	/// <remarks>
	/// This code is not used in the Microsoft.Maui.Controls.Compatibility.Platform.Tizen implementation, however it should not
	/// be removed as it allows developers to use native controls directly.
	/// </remarks>
	public static class LayoutExtensions
	{
		/// <summary>
		/// Add the specified evas object to the list of children views.
		/// </summary>
		/// <param name="children">The extended class.</param>
		/// <param name="obj">Object to be added.</param>
		/// <param name="measureDelegate">Optional delegate which provides measurements for the added object.</param>
		public static void Add(this IList<View> children, NView obj, MeasureDelegate measureDelegate = null)
		{
			children.Add(obj.ToView(measureDelegate));
		}

		/// <summary>
		/// Wraps the evas object into a view which can be used by Xamarin.
		/// </summary>
		/// <returns>The Xamarin view which wraps the evas object.</returns>
		/// <param name="obj">The extended class.</param>
		/// <param name="measureDelegate">Optional delegate which provides measurements for the evas object.</param>
		public static View ToView(this NView obj, MeasureDelegate measureDelegate = null)
		{
			return new NativeViewWrapper(obj, measureDelegate);
		}
	}
}
