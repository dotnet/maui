using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Internals
{
	public static class PropertyPropagationExtensions
	{
		internal static void PropagatePropertyChanged(string propertyName, Element element, IEnumerable children)
		{
			if (propertyName == null || propertyName == VisualElement.FlowDirectionProperty.PropertyName)
				SetFlowDirectionFromParent(element);

			if (propertyName == null || propertyName == VisualElement.VisualProperty.PropertyName)
				SetVisualfromParent(element);

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

		public static void PropagatePropertyChanged(string propertyName, Element target, Element source)
		{
			if (propertyName == null || propertyName == VisualElement.FlowDirectionProperty.PropertyName)
				PropagateFlowDirection(target, source);

			if (propertyName == null || propertyName == VisualElement.VisualProperty.PropertyName)
				PropagateVisual(target, source);

			if (target is IPropertyPropagationController view)
				view.PropagatePropertyChanged(propertyName);
		}

		internal static void PropagateFlowDirection(Element target, Element source)
		{
			IFlowDirectionController controller = target as IFlowDirectionController;
			if (controller == null)
				return;

			var sourceController = source as IFlowDirectionController;
			if (sourceController == null)
				return;

			if (controller.EffectiveFlowDirection.IsImplicit())
			{
				var flowDirection = sourceController.EffectiveFlowDirection.ToFlowDirection();

				if (flowDirection != controller.EffectiveFlowDirection.ToFlowDirection())
				{
					controller.EffectiveFlowDirection = flowDirection.ToEffectiveFlowDirection();
				}
			}

			controller.EffectiveFlowDirection = controller.EffectiveFlowDirection;
		}

		internal static void SetFlowDirectionFromParent(Element child)
		{
			PropagateFlowDirection(child, child.Parent);
		}

		internal static void PropagateVisual(Element target, Element source)
		{
			IVisualController targetController = target as IVisualController;
			if (targetController == null)
				return;

			if (targetController.Visual != VisualMarker.MatchParent)
			{
				targetController.EffectiveVisual = targetController.Visual;
				return;
			}

			if (source is IVisualController sourceController)
				targetController.EffectiveVisual = sourceController.EffectiveVisual;
		}

		internal static void SetVisualfromParent(Element child)
		{
			PropagateVisual(child, child.Parent);
		}
	}
}