using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class PageRenderer : 
		//Microsoft.UI.Xaml.Controls.Grid, IVisualElementRenderer 
		VisualElementRenderer<Page, FrameworkElement>
	{
		bool _disposed;

		bool _loaded;

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			// Pages need an automation peer so we can interact with them in automated tests
			return new FrameworkElementAutomationPeer(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
					ReadOnlyCollection<Element> children = ((IElementController)Element).LogicalChildren;
					for (var i = 0; i < children.Count; i++)
					{
						var visualChild = children[i] as VisualElement;
						visualChild?.Cleanup();
					}
					Element?.SendDisappearing();
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			e.OldElement?.SendDisappearing();

			if (e.NewElement != null)
			{
				if (e.OldElement == null)
				{
					Loaded += OnLoaded;
					Tracker = new BackgroundTracker<FrameworkElement>(BackgroundProperty);
				}

				if (!string.IsNullOrEmpty(Element.AutomationId))
				{
					SetAutomationId(Element.AutomationId);
				}

				if (_loaded)
					e.NewElement.SendAppearing();
			}
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			var carouselPage = Element?.Parent as CarouselPage;
			if (carouselPage != null && carouselPage.Children[0] != Element)
			{
				return;
			}
			_loaded = true;
			Unloaded += OnUnloaded;
			Element?.SendAppearing();
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			Unloaded -= OnUnloaded;
			_loaded = false;
			Element?.SendDisappearing();
		}

		//public PageRenderer()
		//{
		//	//Children.Add(new TextBlock());
		//}

		//public FrameworkElement ContainerElement => this;

		//public VisualElement Element { get; set; }

		//public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		//public void Dispose()
		//{
		//}

		//public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		//{
		//	if (Children.Count == 0)
		//		return new SizeRequest();

		//	var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
		//	var child = this;

		//	child.Measure(constraint);
		//	var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

		//	return new SizeRequest(result);
		//}

		//public UIElement GetNativeElement()
		//{
		//	return this;
		//}

		//public void SetElement(VisualElement element)
		//{
		//	Element = element;
		//	ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, element));
		//}
	}
}