using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : AbstractViewHandler<ITimePicker, MauiTimePicker>
	{
		protected override MauiTimePicker CreateNativeView()
		{
			return new MauiTimePicker(() =>
			{
				SetVirtualViewTime();
				TypedNativeView?.ResignFirstResponder();
			});
		}

		protected override void ConnectHandler(MauiTimePicker nativeView)
		{
			if (nativeView != null)
				nativeView.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(MauiTimePicker nativeView)
		{
			if (nativeView != null)
			{
				nativeView.RemoveFromSuperview();
				nativeView.ValueChanged -= OnValueChanged;
				nativeView.Dispose();
			}
		}

		public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.TypedNativeView?.UpdateFormat(timePicker);
		}

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.TypedNativeView?.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(TimePickerHandler handler, ITimePicker timePicker)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(timePicker, fontManager);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			SetVirtualViewTime();
		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || TypedNativeView == null)
				return;

			VirtualView.Time = TypedNativeView.Date.ToDateTime() - new DateTime(1, 1, 1);
		}
	}
}