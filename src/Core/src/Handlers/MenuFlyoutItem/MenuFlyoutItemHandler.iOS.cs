using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutItemHandler
	{
		internal static Dictionary<int, IMenuElement> menus = new Dictionary<int, IMenuElement>();
		string? _contextText;
		IImageSource? _contextSource;
		bool _refreshContextSource;

		internal void RefreshContextMetadata()
		{
			UpdateValue(nameof(IMenuElement.Text));
			_refreshContextSource = VirtualView.Source is not null;
			try
			{
				UpdateValue(nameof(IMenuElement.Source));
			}
			finally
			{
				_refreshContextSource = false;
			}
		}

		bool IsInContextFlyout()
		{
			IElement? current = VirtualView;
			while (current != null)
			{
				if (current is Microsoft.Maui.IMenuFlyout)
					return true;
				current = current.Parent;
			}
			return false;
		}

		protected override UIMenuElement CreatePlatformElement()
		{
			// https://github.com/dotnet/maui/issues/9332
			// The menu code needs to be converted over to using `UIAction`
			// so that all of this can be the same
			if (IsInContextFlyout())
			{
				UIImage? contextUiImage = VirtualView.Source.GetPlatformMenuImage(MauiContext!);
				_contextText = VirtualView.Text;
				_contextSource = VirtualView.Source;
				var owner = new WeakReference<IMenuFlyoutItem>(VirtualView);

				var uiAction = UIAction.Create(
					title: VirtualView.Text,
					image: contextUiImage,
					identifier: null,
					handler: (_) =>
					{
						if (owner.TryGetTarget(out var item))
							item.Clicked();
					});

				return uiAction;
			}

			return VirtualView.CreateMenuItem(MauiContext!);
		}

		public static void MapIsEnabled(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			handler.PlatformView?.UpdateIsEnabled(view);
		}

		internal static void MapContextText(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			if (handler is MenuFlyoutItemHandler menuHandler && handler.PlatformView is UIAction action &&
				!string.Equals(menuHandler._contextText, view.Text, StringComparison.Ordinal))
			{
				menuHandler._contextText = view.Text;
				action.Title = view.Text;
				menuHandler._mapper.UpdateProperties(menuHandler, view);
			}
		}

		internal static void MapContextSource(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			if (handler is MenuFlyoutItemHandler menuHandler && handler.PlatformView is UIAction action &&
				(menuHandler._refreshContextSource || menuHandler._contextSource != view.Source))
			{
				menuHandler._refreshContextSource = false;
				menuHandler._contextSource = view.Source;
				action.Image = view.Source.GetPlatformMenuImage(handler.MauiContext
					?? throw new InvalidOperationException("The menu item handler must have a MauiContext."));
				menuHandler._mapper.UpdateProperties(menuHandler, view);
			}
		}

		internal static void Execute(UICommand uICommand)
		{
			if (uICommand.PropertyList is NSString nsString &&
				Int32.TryParse(nsString.ToString(), out int index))
			{
				menus[index].Clicked();
			}
		}

		internal static void Reset()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			menus.Clear();
		}
	}
}
