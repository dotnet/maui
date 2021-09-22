using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class CustomNavigationPage : NavigationPage
	{
		public CustomNavigationPage(IServiceProvider services, MainViewModel viewModel) :
			base(new MainPage(services, viewModel))
		{
			InitializeComponent();
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();

#if __ANDROID__

			if (Handler?.NativeView is Android.Views.View layout)
			{
				var view = layout.FindViewById<Android.Views.View>(Resource.Id.maui_toolbar);

				if (view?.LayoutParameters is Google.Android.Material.AppBar.AppBarLayout.LayoutParams layoutParams)
				{
					layoutParams.ScrollFlags =
						Google.Android.Material.AppBar.AppBarLayout.LayoutParams.ScrollFlagScroll
						| Google.Android.Material.AppBar.AppBarLayout.LayoutParams.ScrollFlagEnterAlways;
				}
			}
#endif
		}
	}
}
