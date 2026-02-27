using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers;

public partial class DatePickerHandler : ViewHandler<IDatePicker, CalendarDatePicker>
{
	protected override CalendarDatePicker CreatePlatformView() => new CalendarDatePicker();

	protected override void ConnectHandler(CalendarDatePicker platformView)
	{
		platformView.Opened += Opened;
		platformView.Closed += Closed;
		platformView.DateChanged += DateChanged;
	}

	protected override void DisconnectHandler(CalendarDatePicker platformView)
	{
		platformView.Opened -= Opened;
		platformView.Closed -= Closed;
		platformView.DateChanged -= DateChanged;
	}

	public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView.UpdateDate(datePicker);
	}

	public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView.UpdateDate(datePicker);
	}

	public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView.UpdateMinimumDate(datePicker);
	}

	public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView.UpdateMaximumDate(datePicker);
	}

	public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView.UpdateCharacterSpacing(datePicker);
	}

	public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
	{
		var fontManager = handler.GetRequiredService<IFontManager>();

		handler.PlatformView.UpdateFont(datePicker, fontManager);
	}

	public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView.UpdateTextColor(datePicker);
	}

	internal static partial void MapIsOpen(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView?.UpdateIsOpen(datePicker);
	}

	void DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
	{
		if (VirtualView is null)
		{
			return;
		}

		if (!args.NewDate.HasValue)
		{
			VirtualView.Date = null;
			return;
		}

		if (VirtualView.Date != args.NewDate.Value.DateTime)
		{
			VirtualView.Date = args.NewDate.Value.DateTime;
		}
	}

	void Opened(object? sender, object e)
	{
		if (VirtualView is null)
			return;

		VirtualView.IsOpen = true;
	}

	void Closed(object? sender, object e)
	{
		if (VirtualView is null)
			return;

		VirtualView.IsOpen = false;
	}

	public static partial void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
	{
		handler.PlatformView?.UpdateBackground(datePicker);
	}
}