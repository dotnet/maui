using Tizen.UIExtensions.NUI;
using NEntry = Tizen.UIExtensions.NUI.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler : ViewHandler<IPicker, NEntry>
	{
		protected override NEntry CreatePlatformView() => new NEntry
		{
			IsReadOnly = true
		};

		protected override void ConnectHandler(NEntry platformView)
		{
			platformView.TouchEvent += OnTouch;
			base.ConnectHandler(platformView);
		}
		protected override void DisconnectHandler(NEntry platformView)
		{
			platformView.TouchEvent -= OnTouch;
			base.DisconnectHandler(platformView);
		}

		// Uncomment me on NET7 [Obsolete]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => Reload(handler);

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateTitleColor(picker);
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateHorizontalTextAlignment(picker);
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateVerticalTextAlignment(picker);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateTextColor(picker);
		}

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			handler.PlatformView.UpdateSelectedIndex(picker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(IPickerHandler handler, IPicker picker) { }

		static void Reload(IPickerHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView == null)
				return;

			handler.PlatformView.UpdatePicker(handler.VirtualView);
		}

		bool OnTouch(object source, Tizen.NUI.BaseComponents.View.TouchEventArgs e)
		{
			if (e.Touch.GetState(0) != Tizen.NUI.PointStateType.Up)
				return false;

			if (VirtualView == null)
				return false;

			OpenPopupAsync();
			return true;
		}

		async void OpenPopupAsync()
		{
			if (VirtualView == null)
				return;

			using (var popup = new ActionSheetPopup(VirtualView.Title, "Cancel", null, VirtualView.GetItemsAsArray()))
			{
				try
				{
					VirtualView.SelectedIndex = VirtualView.GetItemsAsArray().IndexOf(await popup.Open());
				}
				catch
				{
					// Cancel
				}
			}
		}

	}
}