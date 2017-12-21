using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.Forms.Platform.WPF.Helpers;

namespace Xamarin.Forms.Platform.WPF
{
	public class LayoutRenderer : ViewRenderer<Layout, FormsPanel>
	{
		IElementController ElementController => Element as IElementController;
		bool _isZChanged;

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsPanel(Element));
				}

				// Update control property 
				UpdateClipToBounds();
				foreach (Element child in ElementController.LogicalChildren)
					HandleChildAdded(Element, new ElementEventArgs(child));

				// Suscribe element event
				Element.ChildAdded += HandleChildAdded;
				Element.ChildRemoved += HandleChildRemoved;
				Element.ChildrenReordered += HandleChildrenReordered;
			}

			base.OnElementChanged(e);
		}

		protected override void Appearing()
		{
			base.Appearing();
			Element.Layout(new Rectangle(0, 0, Control.ActualWidth, Control.ActualHeight));
		}

		void HandleChildAdded(object sender, ElementEventArgs e)
		{
			UiHelper.ExecuteInUiThread(() =>
			{
				var view = e.Element as VisualElement;

				if (view == null)
					return;

				IVisualElementRenderer renderer;
				Platform.SetRenderer(view, renderer = Platform.CreateRenderer(view));
				Control.Children.Add(renderer.GetNativeElement());
				if (_isZChanged)
					EnsureZIndex();
			});
		}

		void HandleChildRemoved(object sender, ElementEventArgs e)
		{
			UiHelper.ExecuteInUiThread(() =>
			{
				var view = e.Element as VisualElement;

				if (view == null)
					return;

				FrameworkElement native = Platform.GetRenderer(view)?.GetNativeElement() as FrameworkElement;
				if (native != null)
				{
					Control.Children.Remove(native);
					view.Cleanup();
					if (_isZChanged)
						EnsureZIndex();
				}
			});
		}

		void HandleChildrenReordered(object sender, EventArgs e)
		{
			EnsureZIndex();
		}

		void EnsureZIndex()
		{
			if (ElementController.LogicalChildren.Count == 0)
				return;

			for (var z = 0; z < ElementController.LogicalChildren.Count; z++)
			{
				var child = ElementController.LogicalChildren[z] as VisualElement;
				if (child == null)
					continue;

				IVisualElementRenderer childRenderer = Platform.GetRenderer(child);

				if (childRenderer == null)
					continue;

				if (Canvas.GetZIndex(childRenderer.GetNativeElement()) != (z + 1))
				{
					if (!_isZChanged)
						_isZChanged = true;

					Canvas.SetZIndex(childRenderer.GetNativeElement(), z + 1);
				}
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Layout.IsClippedToBoundsProperty.PropertyName)
				UpdateClipToBounds();
		}

		protected override void UpdateBackground()
		{
			Control.UpdateDependencyColor(FormsPanel.BackgroundProperty, Element.BackgroundColor);
		}

		protected override void UpdateNativeWidget()
		{
			base.UpdateNativeWidget();
			UpdateClipToBounds();
		}

		void UpdateClipToBounds()
		{
			Control.Clip = null;
			if (Element.IsClippedToBounds)
				Control.Clip = new RectangleGeometry { Rect = new Rect(0, 0, Control.ActualWidth, Control.ActualHeight) };
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Element != null)
				{
					Element.ChildAdded -= HandleChildAdded;
					Element.ChildRemoved -= HandleChildRemoved;
					Element.ChildrenReordered -= HandleChildrenReordered;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
