using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IElementController
	{
		IEffectControlProvider EffectControlProvider { get; set; }

		bool EffectIsAttached(string name);

		void SetValueFromRenderer(BindableProperty property, object value);
		void SetValueFromRenderer(BindablePropertyKey propertyKey, object value);
		ReadOnlyCollection<Element> LogicalChildren { get; }
		Element RealParent { get; }
		IEnumerable<Element> Descendants();

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("IPlatform is obsolete as of 3.5.0. Do not use this property.")]
		IPlatform Platform { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("PlatformSet is obsolete as of 3.5.0. Do not use this event.")]
		event EventHandler PlatformSet;
	}
}