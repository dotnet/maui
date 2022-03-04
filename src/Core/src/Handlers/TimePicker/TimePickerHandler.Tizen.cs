using Tizen.NUI;
using NEntry = Tizen.UIExtensions.NUI.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, NEntry>
	{
		protected override NEntry CreatePlatformView() => new NEntry
		{
			IsReadOnly = true,
			VerticalAlignment = VerticalAlignment.Center,
			Focusable = true,
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

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateFormat(timePicker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateTime(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateTextColor(timePicker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker) { }

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

			using (var popup = new MauiTimePicker(VirtualView.Time))
			{
				try
				{
					VirtualView.Time = await popup.Open();
				}
				catch
				{
					// Cancel
				}
			}
		}

	}
}