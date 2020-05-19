using System.Collections.Generic;

namespace Xamarin.Forms
{
	internal interface IControlTemplated
	{
		ControlTemplate ControlTemplate { get; set; }

		IList<Element> InternalChildren { get; }

		Element TemplateRoot { get; set; }

		void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue);

		void OnApplyTemplate();
	}
}