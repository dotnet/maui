using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	public interface IElementController
	{
		IEffectControlProvider EffectControlProvider { get; set; }

		void SetValueFromRenderer(BindableProperty property, object value);
		void SetValueFromRenderer(BindablePropertyKey propertyKey, object value);
		ReadOnlyCollection<Element> LogicalChildren { get; }
	}
}