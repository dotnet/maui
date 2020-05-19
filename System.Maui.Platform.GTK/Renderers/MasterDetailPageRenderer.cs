using Gtk;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Maui.Internals;
using System.Maui.Platform.GTK.Controls;
using System.Maui.Platform.GTK.Extensions;

namespace System.Maui.Platform.GTK.Renderers
{
	public class MasterDetailPageRenderer : AbstractPageRenderer<Controls.MasterDetailPage, MasterDetailPage>
	{
		Page _currentMaster;
		Page _currentDetail;

		public MasterDetailPageRenderer()
		{
			MessagingCenter.Subscribe(this, System.Maui.Maui.BarTextColor, (NavigationPage sender, Color color) =>
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

			MessagingCenter.Subscribe(this, System.Maui.Maui.BarBackgroundColor, (NavigationPage sender, Color color) =>
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

				MessagingCenter.Unsubscribe<NavigationPage, Color>(this, System.Maui.Maui.BarTextColor);
				MessagingCenter.Unsubscribe<NavigationPage, Color>(this, System.Maui.Maui.BarBackgroundColor);

				if (Page?.Master != null)
				{
					Page.Master.PropertyChanged -= HandleMasterPropertyChanged;
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
					Widget = new Controls.MasterDetailPage();
					var eventBox = new GtkFormsContainer();
					eventBox.Add(Widget);

					Control.Content = eventBox;

					Widget.IsPresentedChanged += OnIsPresentedChanged;

					UpdateMasterDetail();
					UpdateMasterBehavior();
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

			if (e.PropertyName.Equals(nameof(MasterDetailPage.Master)) || e.PropertyName.Equals(nameof(MasterDetailPage.Detail)))
			{
				UpdateMasterDetail();
				UpdateMasterBehavior();
				UpdateIsPresented();
			}
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
				UpdateIsPresented();
			else if (e.PropertyName == MasterDetailPage.MasterBehaviorProperty.PropertyName)
				UpdateMasterBehavior();
		}

		private async void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == System.Maui.Page.IconImageSourceProperty.PropertyName)
				await UpdateHamburguerIconAsync();
		}

		private void UpdateMasterDetail()
		{
			Gtk.Application.Invoke(async delegate
			{
				await UpdateHamburguerIconAsync();
				if (Page.Master != _currentMaster)
				{
					if (_currentMaster != null)
					{
						_currentMaster.PropertyChanged -= HandleMasterPropertyChanged;
					}
					if (Platform.GetRenderer(Page.Master) == null)
						Platform.SetRenderer(Page.Master, Platform.CreateRenderer(Page.Master));
					Widget.Master = Platform.GetRenderer(Page.Master).Container;
					Widget.MasterTitle = Page.Master?.Title ?? string.Empty;
					Page.Master.PropertyChanged += HandleMasterPropertyChanged;
					_currentMaster = Page.Master;
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

		private void UpdateMasterBehavior()
		{
			if (Page.Detail is NavigationPage)
			{
				Widget.MasterBehaviorType = GetMasterBehavior(Page.MasterBehavior);
			}
			else
			{
				// The only way to display Master page is from a toolbar. If we have not access to one,
				// we should force split mode to display menu (as no gestures are implemented).
				Widget.MasterBehaviorType = MasterBehaviorType.Split;
			}

			Widget.DisplayTitle = Widget.MasterBehaviorType != MasterBehaviorType.Split;
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
			return Page.Master.ApplyNativeImageAsync(System.Maui.Page.IconImageSourceProperty, image =>
			{
				Widget.UpdateHamburguerIcon(image);

				if (Page.Detail is NavigationPage navigationPage)
				{
					var navigationRenderer = Platform.GetRenderer(navigationPage) as IToolbarTracker;
					navigationRenderer?.NativeToolbarTracker.UpdateToolBar();
				}
			});
		}

		private MasterBehaviorType GetMasterBehavior(MasterBehavior masterBehavior)
		{
			switch (masterBehavior)
			{
				case MasterBehavior.Split:
				case MasterBehavior.SplitOnLandscape:
				case MasterBehavior.SplitOnPortrait:
					return MasterBehaviorType.Split;
				case MasterBehavior.Popover:
					return MasterBehaviorType.Popover;
				case MasterBehavior.Default:
					return MasterBehaviorType.Default;
				default:
					throw new ArgumentOutOfRangeException(nameof(masterBehavior));
			}
		}

		private void OnIsPresentedChanged(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Widget.IsPresented);
		}
	}
}
