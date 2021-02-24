using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
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
	}
}