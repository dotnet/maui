using System;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	// Currently only inheriting because we can't tap into CreateNativeView
	// Once we can wire up into CreateNativeView then all of this code can move into the 
	// Remap structures
	internal partial class NavigationPageHandler : Microsoft.Maui.Handlers.NavigationViewHandler
	{
		public static PropertyMapper<NavigationPage, NavigationPageHandler> NavigationPageMapper =
			new PropertyMapper<NavigationPage, NavigationPageHandler>(NavigationViewHandler.NavigationViewMapper)
			{
				[NavigationPage.HasNavigationBarProperty.PropertyName] = UpdateToolBar,
				[NavigationPage.HasBackButtonProperty.PropertyName] = UpdateToolBar,
				[NavigationPage.TitleIconImageSourceProperty.PropertyName] = UpdateToolBar,
				[NavigationPage.TitleViewProperty.PropertyName] = UpdateToolBar,
				[NavigationPage.IconColorProperty.PropertyName] = UpdateToolBar,
				[Page.TitleProperty.PropertyName] = UpdateToolBar,
				[NavigationPage.CurrentPageProperty.PropertyName] = UpdateToolBar,
				[PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.BarHeightProperty.PropertyName] = UpdateToolBar,
			};

		private static void UpdateToolBar(NavigationPageHandler arg1, NavigationPage arg2)
		{
			arg1._controlsNavigationManager.ToolbarPropertyChanged();
		}

		ControlsNavigationManager _controlsNavigationManager;
		public NavigationPageHandler() : base(NavigationPageMapper)
		{

		}

		protected override NavigationManager CreateNavigationManager()
			=> _controlsNavigationManager ??= new ControlsNavigationManager();

		//protected override NavigationLayout CreateNativeView()
		//{
		//	LayoutInflater li = LayoutInflater.From(Context);
		//	_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");
		//	var view = li.Inflate(Resource.Layout.navigationlayoutcontrols, null).JavaCast<NavigationPageView>();
		//	return view;
		//}

		//public override void SetVirtualView(IView view)
		//{
		//	base.SetVirtualView(view);
		//	NativeView.SetVirtualView((NavigationPage)view);
		//}
	}
}
