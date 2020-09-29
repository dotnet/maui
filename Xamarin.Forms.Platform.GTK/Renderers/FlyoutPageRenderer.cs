using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Gtk;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class FlyoutPageRenderer : AbstractPageRenderer<Controls.FlyoutPage, FlyoutPage>
	{
		Page _currentFlyout;
		Page _currentDetail;

		public FlyoutPageRenderer()
		{
			MessagingCenter.Subscribe(this, Forms.BarTextColor, (NavigationPage sender, Color color) =>
			{
				var barTextColor = color;

				if (barTextColor == null || barTextColor.IsDefaultOrTransparent())
				{
					Widget.UpdateBarTextColor(null);
				}
				else
				{
					Widget.UpdateBarTextColor(color.ToGtkColor());
				}
			});

			MessagingCenter.Subscribe(this, Forms.BarBackgroundColor, (NavigationPage sender, Color color) =>
			{
				var barBackgroundColor = color;

				if (barBackgroundColor == null || barBackgroundColor.IsDefaultOrTransparent())
				{
					Widget.UpdateBarBackgroundColor(null);
				}
				else
				{
					Widget.UpdateBarBackgroundColor(color.ToGtkColor());
				}
			});
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Widget != null)
				{
					Widget.IsPresentedChanged -= OnIsPresentedChanged;
				}

				MessagingCenter.Unsubscribe<NavigationPage, Color>(this, Forms.BarTextColor);
				MessagingCenter.Unsubscribe<NavigationPage, Color>(this, Forms.BarBackgroundColor);

				if (Page?.Flyout != null)
				{
					Page.Flyout.PropertyChanged -= HandleFlyoutPropertyChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Widget == null)
				{
					// There is nothing similar in Gtk. 
					// Custom control has been created that simulates the expected behavior.
					Widget = new Controls.FlyoutPage();
					var eventBox = new GtkFormsContainer();
					eventBox.Add(Widget);

					Control.Content = eventBox;

					Widget.IsPresentedChanged += OnIsPresentedChanged;

					UpdateFlyoutPage();
					UpdateFlyoutLayoutBehavior();
					UpdateIsPresented();
					UpdateBarTextColor();
					UpdateBarBackgroundColor();
				}
			}
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			Control?.Content?.SetSize(allocation.Width, allocation.Height);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName.Equals(nameof(FlyoutPage.Flyout)) || e.PropertyName.Equals(nameof(FlyoutPage.Detail)))
			{
				UpdateFlyoutPage();
				UpdateFlyoutLayoutBehavior();
				UpdateIsPresented();
			}
			else if (e.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName)
				UpdateIsPresented();
			else if (e.PropertyName == FlyoutPage.FlyoutLayoutBehaviorProperty.PropertyName)
				UpdateFlyoutLayoutBehavior();
		}

		private async void HandleFlyoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Xamarin.Forms.Page.IconImageSourceProperty.PropertyName)
				await UpdateHamburguerIconAsync();
		}

		private void UpdateFlyoutPage()
		{
			Gtk.Application.Invoke(async delegate
			{
				await UpdateHamburguerIconAsync();
				if (Page.Flyout != _currentFlyout)
				{
					if (_currentFlyout != null)
					{
						_currentFlyout.PropertyChanged -= HandleFlyoutPropertyChanged;
					}
					if (Platform.GetRenderer(Page.Flyout) == null)
						Platform.SetRenderer(Page.Flyout, Platform.CreateRenderer(Page.Flyout));
					Widget.Flyout = Platform.GetRenderer(Page.Flyout).Container;
					Widget.FlyoutTitle = Page.Flyout?.Title ?? string.Empty;
					Page.Flyout.PropertyChanged += HandleFlyoutPropertyChanged;
					_currentFlyout = Page.Flyout;
				}
				if (Page.Detail != _currentDetail)
				{
					if (Platform.GetRenderer(Page.Detail) == null)
						Platform.SetRenderer(Page.Detail, Platform.CreateRenderer(Page.Detail));
					Widget.Detail = Platform.GetRenderer(Page.Detail).Container;
					_currentDetail = Page.Detail;
				}
				UpdateBarTextColor();
				UpdateBarBackgroundColor();
			});
		}

		private void UpdateIsPresented()
		{
			Widget.IsPresented = Page.IsPresented;
		}

		private void UpdateFlyoutLayoutBehavior()
		{
			if (Page.Detail is NavigationPage)
			{
				Widget.FlyoutLayoutBehaviorType = GetFlyoutLayoutBehavior(Page.FlyoutLayoutBehavior);
			}
			else
			{
				// The only way to display Flyout page is from a toolbar. If we have not access to one,
				// we should force split mode to display menu (as no gestures are implemented).
				Widget.FlyoutLayoutBehaviorType = FlyoutLayoutBehaviorType.Split;
			}

			Widget.DisplayTitle = Widget.FlyoutLayoutBehaviorType != FlyoutLayoutBehaviorType.Split;
		}

		private void UpdateBarTextColor()
		{
			var navigationPage = Page.Detail as NavigationPage;

			if (navigationPage != null)
			{
				var barTextColor = navigationPage.BarTextColor;

				Widget.UpdateBarTextColor(barTextColor.ToGtkColor());
			}
		}

		private void UpdateBarBackgroundColor()
		{
			var navigationPage = Page.Detail as NavigationPage;

			if (navigationPage != null)
			{
				var barBackgroundColor = navigationPage.BarBackgroundColor;
				Widget.UpdateBarBackgroundColor(barBackgroundColor.ToGtkColor());
			}
		}

		private Task UpdateHamburguerIconAsync()
		{
			return Page.Flyout.ApplyNativeImageAsync(Xamarin.Forms.Page.IconImageSourceProperty, image =>
			{
				Widget.UpdateHamburguerIcon(image);

				if (Page.Detail is NavigationPage navigationPage)
				{
					var navigationRenderer = Platform.GetRenderer(navigationPage) as IToolbarTracker;
					navigationRenderer?.NativeToolbarTracker.UpdateToolBar();
				}
			});
		}

		private FlyoutLayoutBehaviorType GetFlyoutLayoutBehavior(FlyoutLayoutBehavior flyoutBehavior)
		{
			switch (flyoutBehavior)
			{
				case FlyoutLayoutBehavior.Split:
				case FlyoutLayoutBehavior.SplitOnLandscape:
				case FlyoutLayoutBehavior.SplitOnPortrait:
					return FlyoutLayoutBehaviorType.Split;
				case FlyoutLayoutBehavior.Popover:
					return FlyoutLayoutBehaviorType.Popover;
				case FlyoutLayoutBehavior.Default:
					return FlyoutLayoutBehaviorType.Default;
				default:
					throw new ArgumentOutOfRangeException(nameof(flyoutBehavior));
			}
		}

		private void OnIsPresentedChanged(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(FlyoutPage.IsPresentedProperty, Widget.IsPresented);
		}
	}

	public class MasterDetailPageRenderer : FlyoutPageRenderer
	{

	}
}
