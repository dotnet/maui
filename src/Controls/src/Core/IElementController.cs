#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>For internal use by .NET MAUI.</summary>
	public interface IElementController
	{
		/// <summary>For internal use by .NET MAUI.</summary>
		IEffectControlProvider EffectControlProvider { get; set; }

		/// <summary>For internal use by .NET MAUI.</summary>
		/// <param name="name">The name of the effect.</param>
		/// <remarks><langword>true</langword> if the effect is attached to the element, <langword>false</langword> otherwise.</remarks>
		bool EffectIsAttached(string name);

		/// <summary>For internal use by .NET MAUI.</summary>
		void SetValueFromRenderer(BindableProperty property, object value);

		/// <summary>For internal use by .NET MAUI.</summary>
		void SetValueFromRenderer(BindablePropertyKey propertyKey, object value);

		/// <summary>For internal use by .NET MAUI.</summary>
		/// <remarks>Gets a readonly list of the element's logical children.</remarks>
		IReadOnlyList<Element> LogicalChildren { get; }

		/// <summary>For internal use by .NET MAUI.</summary>
		Element RealParent { get; }

		/// <summary>For internal use by .NET MAUI.</summary>
		/// <remarks>Gets a collection of the element's descendants.</remarks>
		IEnumerable<Element> Descendants();
	}
}