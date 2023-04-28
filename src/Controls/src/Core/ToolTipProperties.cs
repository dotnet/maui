using System;

namespace Microsoft.Maui.Controls
{
	public class ToolTipProperties
	{
		/// <summary>Bindable property for attached property <c>Text</c>.</summary>
		public static readonly BindableProperty TextProperty =
 			BindableProperty.CreateAttached("Text", typeof(string), typeof(ToolTipProperties), defaultValue: null, propertyChanged: OnToolTipPropertyChanged);

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

		internal static ToolTip? GetToolTip(BindableObject bindable)
		{
			if (!bindable.IsSet(TextProperty))
				return null;

			return new ToolTip()
			{
				Content = GetText(bindable)
			};
		}
	}
}
#nullable disable
