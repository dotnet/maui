using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Handlers
{

	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, MauiNavigationView>
	{
		public static IPropertyMapper<IFlyoutView, FlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, FlyoutViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IFlyoutView.Flyout)] = MapFlyout,
			[nameof(IFlyoutView.Detail)] = MapDetail,
		};

		readonly FlyoutPanel _flyoutPanel;

		public FlyoutViewHandler() : base(Mapper)
		{
			_flyoutPanel = new FlyoutPanel();
		}

		protected override MauiNavigationView CreateNativeView()
		{
			var navigationView = new MauiNavigationView();

			navigationView.PaneFooter = _flyoutPanel;
			navigationView.FlyoutPaneSizeChanged += (_, __) =>
			{
				_flyoutPanel.Height = NativeView.FlyoutPaneSize.Height;
				_flyoutPanel.Width = NativeView.FlyoutPaneSize.Width;
			};

			return navigationView;
		}

		void UpdateDetail()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Detail.ToNative(MauiContext);

			NativeView.Content = VirtualView.Detail.GetNative(true);
		}
		
		void UpdateFlyout()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Flyout.ToNative(MauiContext);
			
			_flyoutPanel.Children.Clear();

			if(VirtualView.Flyout.GetNative(true) is UIElement element)
				_flyoutPanel.Children.Add(element);
		}

		public static void MapDetail(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateDetail();
		}

		public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateFlyout();
		}

		// We use a container because if we just assign our Flyout to the PaneFooter on the NavigationView 
		// The measure call passes in PositiveInfinity for the measurements which causes the layout system
		// to crash. So we use this Panel to fascillitate more constrained measuring values
		class FlyoutPanel : Panel
		{
			public FlyoutPanel()
			{
				Height = 0;
				Width = 0;
			}

			FrameworkElement? FlyoutContent =>
				Children.Count > 0 ? (FrameworkElement?)Children[0] : null;

			protected override Size MeasureOverride(Size availableSize)
			{
				if (FlyoutContent == null)
					return Size.Empty;

				FlyoutContent.Measure(availableSize);
				return FlyoutContent.DesiredSize;
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				if (FlyoutContent == null)
					return Size.Empty;

				FlyoutContent.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
				return new Size(FlyoutContent.ActualWidth, FlyoutContent.ActualHeight);
			}
		}
	}
}
