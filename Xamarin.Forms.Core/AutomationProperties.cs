namespace Xamarin.Forms
{
	public class AutomationProperties
	{
		public static readonly BindableProperty HelpTextProperty = BindableProperty.Create("HelpText", typeof(string), typeof(AutomationProperties), default(string));

		public static readonly BindableProperty IsInAccessibleTreeProperty = BindableProperty.Create("IsInAccessibleTree", typeof(bool?), typeof(AutomationProperties), null);

		public static readonly BindableProperty LabeledByProperty = BindableProperty.Create("LabeledBy", typeof(VisualElement), typeof(AutomationProperties), default(VisualElement));

		public static readonly BindableProperty NameProperty = BindableProperty.Create("Name", typeof(string), typeof(AutomationProperties), default(string));

		public static string GetHelpText(BindableObject bindable)
		{
			return (string)bindable.GetValue(HelpTextProperty);
		}

		public static bool? GetIsInAccessibleTree(BindableObject bindable)
		{
			return (bool?)bindable.GetValue(IsInAccessibleTreeProperty);
		}

		[TypeConverter(typeof(ReferenceTypeConverter))]
		public static VisualElement GetLabeledBy(BindableObject bindable)
		{
			return (VisualElement)bindable.GetValue(LabeledByProperty);
		}

		public static string GetName(BindableObject bindable)
		{
			return (string)bindable.GetValue(NameProperty);
		}

		public static void SetHelpText(BindableObject bindable, string value)
		{
			bindable.SetValue(HelpTextProperty, value);
		}

		public static void SetIsInAccessibleTree(BindableObject bindable, bool? value)
		{
			bindable.SetValue(IsInAccessibleTreeProperty, value);
		}

		public static void SetLabeledBy(BindableObject bindable, VisualElement value)
		{
			bindable.SetValue(LabeledByProperty, value);
		}

		public static void SetName(BindableObject bindable, string value)
		{
			bindable.SetValue(NameProperty, value);
		}
	}
}