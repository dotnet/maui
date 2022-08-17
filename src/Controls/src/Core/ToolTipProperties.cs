#nullable enable

using System;

namespace Microsoft.Maui.Controls
{
	public class ToolTipProperties
	{
		public static readonly BindableProperty ContentProperty =
 			BindableProperty.CreateAttached("Content", typeof(object), typeof(ToolTipProperties), defaultValue: null, propertyChanged: OnToolTipPropertyChanged);

		static void OnToolTipPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IElement element)
				element?.Handler?.UpdateValue(nameof(IToolTipElement.ToolTip));
		}

		public static object GetContent(BindableObject bindable)
		{
			return (object)bindable.GetValue(ContentProperty);
		}

		public static void SetContent(BindableObject bindable, object value)
		{
			bindable.SetValue(ContentProperty, value);
		}

		internal static ToolTip? GetToolTip(BindableObject bindable)
		{
			if (!bindable.IsSet(ContentProperty))
				return null;

			return new ToolTip()
			{
				Content = GetContent(bindable)
			};
		}
	}
}
#nullable disable