using System.Collections.Generic;

namespace Xamarin.Forms
{
	internal interface IControlTemplated
	{
		ControlTemplate ControlTemplate { get; set; }

		IList<Element> InternalChildren { get; }

		void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue);
	}
}