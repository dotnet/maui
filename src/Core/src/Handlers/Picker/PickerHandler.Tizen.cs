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

		protected override void ConnectHandler(NEntry nativeView)
		{
			nativeView.TouchEvent += OnTouch;
			base.ConnectHandler(nativeView);
		}
		protected override void DisconnectHandler(NEntry platformView)
		{
			platformView.TouchEvent -= OnTouch;
			base.DisconnectHandler(nativeView);
		}

		// Uncomment me on NET7 [Obsolete]
		public static void MapReload(IPickerHandler handler, IPicker picker, object? args) => handler.Reload();

		internal static void MapItems(IPickerHandler handler, IPicker picker) => Reload(handler);

		public static void MapTitleColor(IPickerHandler handler, IPicker picker)
		{
			handler.NativeView.UpdateTitleColor(picker);
		}

		public static void MapFont(IPickerHandler handler, IPicker picker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView.UpdateFont(picker, fontManager);
		}

		public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.NativeView.UpdateHorizontalTextAlignment(picker);
		}

		public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker picker)
		{
			handler.NativeView.UpdateVerticalTextAlignment(picker);
		}

		public static void MapTextColor(IPickerHandler handler, IPicker picker)
		{
			handler.NativeView.UpdateTextColor(picker);
		}

		public static void MapTitle(IPickerHandler handler, IPicker picker)
		{
			handler.NativeView.UpdateTitle(picker);
		}

		public static void MapSelectedIndex(IPickerHandler handler, IPicker picker)
		{
			handler.NativeView.UpdateSelectedIndex(picker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(PickerHandler handler, IPicker picker) { }

		void Reload()
		{
			if (VirtualView == null || NativeView == null)
				return;

			NativeView.UpdatePicker(VirtualView);
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