#nullable disable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	internal interface IControlTemplated
	{
		ControlTemplate ControlTemplate { get; set; }

		IReadOnlyList<Element> InternalChildren { get; }

		void AddLogicalChild(Element element);
		bool RemoveLogicalChild(Element element);
		bool RemoveAt(int index);

		Element TemplateRoot { get; set; }

		void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue);

		void OnApplyTemplate();
	}
}