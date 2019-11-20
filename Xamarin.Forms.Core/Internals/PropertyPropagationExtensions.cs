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

			if (propertyName == null || propertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				BaseShellItem.PropagateFromParent(Shell.NavBarIsVisibleProperty, element);

			if (propertyName == null || propertyName == Shell.NavBarHasShadowProperty.PropertyName)
				BaseShellItem.PropagateFromParent(Shell.NavBarHasShadowProperty, element);

			if (propertyName == null || propertyName == Shell.TabBarIsVisibleProperty.PropertyName)
				BaseShellItem.PropagateFromParent(Shell.TabBarIsVisibleProperty, element);

			foreach (var child in children)
			{
				if (child is IPropertyPropagationController view)
					view.PropagatePropertyChanged(propertyName);
			}
		}

		internal static void PropagatePropertyChanged(string propertyName, Element element)
		{
			if (propertyName == null || propertyName == VisualElement.FlowDirectionProperty.PropertyName)
				Element.SetFlowDirectionFromParent(element);

			if (propertyName == null || propertyName == VisualElement.VisualProperty.PropertyName)
				Element.SetVisualfromParent(element);

			if (element is IPropertyPropagationController view)
					view.PropagatePropertyChanged(propertyName);
		}
	}
}
