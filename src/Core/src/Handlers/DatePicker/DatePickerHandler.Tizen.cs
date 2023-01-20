using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using NEntry = Tizen.UIExtensions.NUI.Entry;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, NEntry>
	{
		protected override NEntry CreatePlatformView() => new MauiPicker();

		protected override void ConnectHandler(NEntry platformView)
		{
			platformView.TouchEvent += OnTouch;
			platformView.KeyEvent += OnKeyEvent;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(NEntry platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.TouchEvent -= OnTouch;
			platformView.KeyEvent -= OnKeyEvent;
			base.DisconnectHandler(platformView);
		}

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateFormat(datePicker);
		}

		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateTextColor(datePicker);
		}

		[MissingMapper]
		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateCharacterSpacing(datePicker);
		}

		bool OnTouch(object source, Tizen.NUI.BaseComponents.View.TouchEventArgs e)
		{
			if (e.Touch.GetState(0) != PointStateType.Up)
				return false;

			if (VirtualView == null)
				return false;

			OpenPopupAsync();
			return true;
		}

		bool OnKeyEvent(object source, Tizen.NUI.BaseComponents.View.KeyEventArgs e)
		{
			if (e.Key.IsAcceptKeyEvent())
			{
				OpenPopupAsync();
				return true;
			}
			return false;
		}

		async void OpenPopupAsync()
		{
			if (VirtualView == null)
				return;

			var modalStack = MauiContext?.GetModalStack();
			if (modalStack != null)
			{
				await modalStack.PushDummyPopupPage(async () =>
				{
					try
					{
						using var popup = new MauiDateTimePicker(VirtualView.Date, false);
						VirtualView.Date = await popup.Open();
					}
					catch
					{
						// Cancel
					}
				});
			}
		}
	}
}