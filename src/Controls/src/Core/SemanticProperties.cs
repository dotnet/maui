namespace Microsoft.Maui.Controls
{
	public class SemanticProperties
	{
		public static readonly BindableProperty DescriptionProperty = BindableProperty.CreateAttached("Description", typeof(string), typeof(SemanticProperties), default(string), propertyChanged : OnSemanticPropertyChanged);

		public static readonly BindableProperty HintProperty = BindableProperty.CreateAttached("Hint", typeof(string), typeof(SemanticProperties), default(string), propertyChanged: OnSemanticPropertyChanged);

		static void OnSemanticPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IFrameworkElement fe)
				fe.Handler?.UpdateValue(nameof(IView.Semantics));
		}

		public static string GetDescription(BindableObject bindable)
		{
			return (string)bindable.GetValue(DescriptionProperty);
		}

		public static void SetDescription(BindableObject bindable, string value)
		{
			bindable.SetValue(DescriptionProperty, value);
		}

		public static string GetHint(BindableObject bindable)
		{
			return (string)bindable.GetValue(HintProperty);
		}

		public static void SetHint(BindableObject bindable, string value)
		{
			bindable.SetValue(HintProperty, value);
		}
	}
}
