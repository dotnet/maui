using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class FormsPanel : Panel
	{
		IElementController ElementController => Element as IElementController;

		public Layout Element { get; private set; }

		public FormsPanel(Layout element)
		{
			Element = element;
		}

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			if (Element == null)
				return finalSize;

			Element.IsInNativeLayout = true;

			var presentationSource = PresentationSource.FromVisual(this);
			var stepX = presentationSource.CompositionTarget.TransformFromDevice.M11;
			var stepY = presentationSource.CompositionTarget.TransformFromDevice.M22;

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i] as VisualElement;
				if (child == null)
					continue;

				IVisualElementRenderer renderer = Platform.GetRenderer(child);
				if (renderer == null)
					continue;
				Rectangle bounds = child.Bounds;
				var control = renderer.GetNativeElement();
				var width = Math.Max(0, bounds.Width);
				var height = Math.Max(0, bounds.Height);
				if (stepX != 1 && stepY != 1 && stepX != 0 && stepY != 0)
				{
					control.Width = width = Math.Ceiling(width / stepX) * stepX;
					control.Height = height = Math.Ceiling(height / stepY) * stepY;
				}
				control.Arrange(new Rect(bounds.X, bounds.Y, width, height));
			}

			Element.IsInNativeLayout = false;

			return finalSize;
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			if (Element == null || availableSize.Width * availableSize.Height == 0)
				return new System.Windows.Size(0, 0);

			Element.IsInNativeLayout = true;

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i] as VisualElement;
				if (child == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(child);
				if (renderer == null)
					continue;

				FrameworkElement control = renderer.GetNativeElement();

				if (control.ActualWidth != child.Width || control.ActualHeight != child.Height)
				{
					double width = child.Width <= -1 ? ActualWidth : child.Width;
					double height = child.Height <= -1 ? ActualHeight : child.Height;
					control.Measure(new System.Windows.Size(width, height));
				}
			}

			System.Windows.Size result;
			if (double.IsInfinity(availableSize.Width) || double.IsPositiveInfinity(availableSize.Height))
			{
				Size request = Element.Measure(availableSize.Width, availableSize.Height, MeasureFlags.IncludeMargins).Request;
				result = new System.Windows.Size(request.Width, request.Height);
			}
			else
			{
				result = availableSize;
			}
			Element.IsInNativeLayout = false;

			if (Double.IsPositiveInfinity(result.Height))
				result.Height = 0.0;
			if (Double.IsPositiveInfinity(result.Width))
				result.Width = 0.0;

			return result;
		}
	}
}
