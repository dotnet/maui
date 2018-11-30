using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Internals
{
	internal static class PropertyPropagationExtensions
	{
		public static void PropagatePropertyChanged(string propertyName, Element element, IEnumerable children)
		{
			if (propertyName == null || propertyName == VisualElement.FlowDirectionProperty.PropertyName)
				Element.SetFlowDirectionFromParent(element);

			if (propertyName == null || propertyName == VisualElement.VisualProperty.PropertyName)
				Element.SetVisualfromParent(element);

			foreach (var child in children)
			{
				if (child is IPropertyPropagationController view)
					view.PropagatePropertyChanged(propertyName);
			}
		}
	}
}
