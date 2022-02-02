using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		protected override ShellView CreateNativeView()
		{
			return new ShellView();
		}

		protected override void ConnectHandler(ShellView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.PaneOpening += OnPaneOpening;
		}

		void OnPaneOpening(UI.Xaml.Controls.NavigationView sender, object args)
		{
			UpdateValue(nameof(Shell.FlyoutBackground));
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			NativeView.SetElement((Shell)view);
		}
		
		public static void MapCurrentItem(ShellHandler handler, Shell view)
		{
			handler.NativeView.SwitchShellItem(view.CurrentItem, true);
		}

		public static void MapFlyoutIsPresented(ShellHandler handler, Shell view)
		{
		}

		public static void MapFlyoutBackgroundColor(ShellHandler handler, Shell view)
		{

		}

		public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell view)
		{
		}

		public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			//TODO MAUI: this maps to ShellContent on Flyout
		}

		public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (flyoutView.Width >= 0)
				handler.NativeView.OpenPaneLength = flyoutView.Width;
			else
				handler.NativeView.OpenPaneLength = 320;
			// At some point this Template Setting is going to show up with a bump to winui
			//handler.NativeView.OpenPaneLength = handler.NativeView.TemplateSettings.OpenPaneWidth;

		}

		public static void MapFlyoutBehavior(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			//var nativeView = handler.NativeView;
			//nativeView.UpdateFlyoutBehavior(flyoutView);
			//handler.UpdateFlyoutPanelMargin();
		}
	}
}
