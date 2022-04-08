#nullable enable

using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		IPlatformViewHandler? _nativeTitleViewHandler;
		MauiToolbar PlatformView => Handler?.PlatformView as MauiToolbar ?? throw new InvalidOperationException("Native View not set");

		public static void MapBarTextColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarTextColor(arg2);
		}

		public static void MapBarBackgroundColor(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBarBackgroundColor(arg2);
		}

		public static void MapBackButtonTitle(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapToolbarItems(ToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateMenu();
		}

		public static void MapTitle(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}

		public static void MapTitleView(ToolbarHandler arg1, Toolbar arg2)
		{
			arg2.UpdateTitleView();
		}

		public static void MapTitleIcon(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateTitleIcon(arg2);
		}

		public static void MapBackButtonVisible(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateBackButton(arg2);
		}

		public static void MapIsVisible(ToolbarHandler arg1, Toolbar arg2)
		{
			arg1.PlatformView.UpdateIsVisible(arg2);
		}

		[MissingMapper]
		public static void MapBarBackground(ToolbarHandler arg1, Toolbar arg2) { }

		[MissingMapper]
		public static void MapIconColor(ToolbarHandler arg1, Toolbar arg2) { }

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
