#nullable enable

using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		IPlatformViewHandler? _nativeTitleViewHandler;
		MauiToolbar PlatformView => Handler?.PlatformView as MauiToolbar ?? throw new InvalidOperationException("Native View not set");

		public static void MapBarTextColor(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateBarTextColor(toolbar);
		}

		public static void MapBarBackgroundColor(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateBarBackgroundColor(toolbar);
		}

		public static void MapBackButtonTitle(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateBackButton(toolbar);
		}

		public static void MapToolbarItems(IToolbarHandler handler, Toolbar toolbar)
		{
			toolbar.UpdateMenu();
		}

		public static void MapTitle(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateTitle(toolbar);
		}

		public static void MapTitleView(IToolbarHandler handler, Toolbar toolbar)
		{
			toolbar.UpdateTitleView();
		}

		public static void MapTitleIcon(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateTitleIcon(toolbar);
		}

		public static void MapBackButtonVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateBackButton(toolbar);
		}

		public static void MapIsVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateIsVisible(toolbar);
		}

		public static void MapBarBackground(IToolbarHandler handler, Toolbar toolbar)
		{
			handler.PlatformView.UpdateBarBackgroundColor(toolbar);
		}

		[MissingMapper]
		public static void MapIconColor(IToolbarHandler handler, Toolbar toolbar) { }

		void UpdateTitleView()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));

			VisualElement titleView = TitleView;
			if (_nativeTitleViewHandler != null)
			{
				PlatformView.Content = null;
				_nativeTitleViewHandler.Dispose();
				_nativeTitleViewHandler = null;
			}

			if (titleView == null)
			{
				PlatformView.UpdateTitle(this);
				return;
			}

			var nativeTitleView = titleView.ToPlatform(MauiContext);
			_nativeTitleViewHandler = titleView.Handler as IPlatformViewHandler;

			PlatformView.Title = string.Empty;
			PlatformView.Content = nativeTitleView;
		}

		void UpdateMenu()
		{
			_ = MauiContext ?? throw new ArgumentNullException(nameof(MauiContext));
			PlatformView.UpdateMenuItems(this);
		}
	}
}
