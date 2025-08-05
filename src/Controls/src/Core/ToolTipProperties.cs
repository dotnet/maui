using System;

namespace Microsoft.Maui.Controls
{
	public class ToolTipProperties
	{
		/// <summary>Bindable property for attached property <c>Text</c>.</summary>
		public static readonly BindableProperty TextProperty =
 			BindableProperty.CreateAttached("Text", typeof(string), typeof(ToolTipProperties), defaultValue: null, propertyChanged: OnToolTipPropertyChanged);

		/// <summary>Bindable property for attached property <c>Delay</c>.</summary>
		public static readonly BindableProperty DelayProperty =
			BindableProperty.CreateAttached("Delay", typeof(int?), typeof(ToolTipProperties), defaultValue: null, propertyChanged: OnToolTipPropertyChanged);

		/// <summary>Bindable property for attached property <c>Duration</c>.</summary>
		public static readonly BindableProperty DurationProperty =
			BindableProperty.CreateAttached("Duration", typeof(int?), typeof(ToolTipProperties), defaultValue: null, propertyChanged: OnToolTipPropertyChanged);

		static void OnToolTipPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IElement element)
				element?.Handler?.UpdateValue(nameof(IToolTipElement.ToolTip));
		}

		public static object GetText(BindableObject bindable)
		{
			return (object)bindable.GetValue(TextProperty);
		}

		public static void SetText(BindableObject bindable, object value)
		{
			bindable.SetValue(TextProperty, value);
		}

		public static int? GetDelay(BindableObject bindable)
		{
			return (int?)bindable.GetValue(DelayProperty);
		}

		public static void SetDelay(BindableObject bindable, int? value)
		{
			bindable.SetValue(DelayProperty, value);
		}

		public static int? GetDuration(BindableObject bindable)
		{
			return (int?)bindable.GetValue(DurationProperty);
		}

		public static void SetDuration(BindableObject bindable, int? value)
		{
			bindable.SetValue(DurationProperty, value);
		}

		internal static ToolTip? GetToolTip(BindableObject bindable)
		{
			if (!bindable.IsSet(TextProperty))
				return null;

			return new ToolTip()
			{
				Content = GetText(bindable),
				Delay = GetDelay(bindable),
				Duration = GetDuration(bindable)
			};
		}
	}
}
#nullable disable
