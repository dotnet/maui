using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		IPlatform Platform { get; set; }
		Element RealParent { get; }
		IEnumerable<Element> Descendants();
		event EventHandler PlatformSet;
	}
}