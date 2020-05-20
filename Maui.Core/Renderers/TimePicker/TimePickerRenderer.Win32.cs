using System.Maui.Core.Controls;
using System.Windows;
using System.Windows.Media;

namespace System.Maui.Platform
{
	public partial class TimePickerRenderer : AbstractViewRenderer<ITimePicker, MauiTimePicker>
	{
		Brush _defaultBrush;
		FontFamily _defaultFontFamily;

		protected override MauiTimePicker CreateView()
		{
			var picker = new MauiTimePicker();
			picker.TimeChanged += OnControlTimeChanged;
			picker.Loaded += OnControlLoaded;
			return picker;
		}

		protected override void SetupDefaults()
		{
			base.SetupDefaults();
		}

		protected override void DisposeView(MauiTimePicker nativeView)
		{
			nativeView.TimeChanged -= OnControlTimeChanged;
			nativeView.Loaded -= OnControlLoaded;
			base.DisposeView(nativeView);
		}

		public static void MapPropertySelectedTime(IViewRenderer renderer, ITimePicker timePicker) => (renderer as TimePickerRenderer)?.UpdateTime();
		public static void MapPropertyColor(IViewRenderer renderer, ITimePicker timePicker) => (renderer as TimePickerRenderer)?.UpdateTextColor();

		public virtual void UpdateTime()
		{
			TypedNativeView.Time = VirtualView.SelectedTime;
		}

		public virtual void UpdateTextColor()
		{
			Color color = VirtualView.Color;
			TypedNativeView.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			// The defaults from the control template won't be available
			// right away; we have to wait until after the template has been applied
			_defaultBrush = TypedNativeView.Foreground;
			_defaultFontFamily = TypedNativeView.FontFamily;	
		}

		void OnControlTimeChanged(object sender, TimeChangedEventArgs e)
		{
			VirtualView.SelectedTime = e.NewTime.HasValue ? e.NewTime.Value : default(TimeSpan);
		}

		void UpdateTimeFormat()
		{
			
		}
	}
}
