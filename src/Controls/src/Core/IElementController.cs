#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	public interface IElementController
	{
		IEffectControlProvider EffectControlProvider { get; set; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		/// <param name="name">The name of the effect.</param>
		/// <remarks><langword>true</langword> if the effect is attached to the element, <langword>false</langword> otherwise.</remarks>
		bool EffectIsAttached(string name);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void SetValueFromRenderer(BindableProperty property, object value);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void SetValueFromRenderer(BindablePropertyKey propertyKey, object value);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		/// <remarks>Gets a readonly list of the element's logical children.</remarks>
		IReadOnlyList<Element> LogicalChildren { get; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		Element RealParent { get; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		/// <remarks>Gets a collection of the element's descendants.</remarks>
		IEnumerable<Element> Descendants();
	}
}