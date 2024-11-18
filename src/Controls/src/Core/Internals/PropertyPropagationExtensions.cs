#nullable disable
using System.Collections;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PropertyPropagationExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.PropertyPropagationExtensions']/Docs/*" />
	public static class PropertyPropagationExtensions
	{
		internal static void PropagatePropertyChanged(string propertyName, Element element, IEnumerable children)
		{
			if (propertyName == null || propertyName == VisualElement.FlowDirectionProperty.PropertyName)
				SetFlowDirectionFromParent(element);

			if (propertyName == null || propertyName == VisualElement.VisualProperty.PropertyName)
				SetVisualFromParent(element);

			if (propertyName == null || propertyName == VisualElement.WindowProperty.PropertyName)
				SetWindowFromParent(element);

			if (propertyName == null || propertyName == Shell.NavBarHasShadowProperty.PropertyName)
				BaseShellItem.PropagateFromParent(Shell.NavBarHasShadowProperty, element);

			if (propertyName == null || propertyName == Shell.TabBarIsVisibleProperty.PropertyName)
				BaseShellItem.PropagateFromParent(Shell.TabBarIsVisibleProperty, element);

			if (propertyName == null || propertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				BaseShellItem.PropagateFromParent(Shell.NavBarIsVisibleProperty, element);

			foreach (var child in children)
			{
				if (child is IPropertyPropagationController view)
					view.PropagatePropertyChanged(propertyName);
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PropertyPropagationExtensions.xml" path="//Member[@MemberName='PropagatePropertyChanged']/Docs/*" />
		public static void PropagatePropertyChanged(string propertyName, Element target, Element source)
		{
			if (propertyName == null || propertyName == VisualElement.FlowDirectionProperty.PropertyName)
				PropagateFlowDirection(target, source);

			if (propertyName == null || propertyName == VisualElement.VisualProperty.PropertyName)
				PropagateVisual(target, source);

			if (propertyName == null || propertyName == VisualElement.WindowProperty.PropertyName)
				PropagateWindow(target, source);

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

		internal static void SetVisualFromParent(Element child)
		{
			PropagateVisual(child, child.Parent);
		}

		internal static void PropagateWindow(Element target, Element source)
		{
			var controller = target as IWindowController;
			if (controller == null)
				return;

			var sourceController = source as IWindowController;

			controller.Window = sourceController?.Window;
		}

		internal static void SetWindowFromParent(Element child)
		{
			PropagateWindow(child, child.Parent);
		}
	}
}
