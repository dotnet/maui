using System.ComponentModel;

namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.VisualElement;
	public static class VisualElement
	{
		public static readonly BindableProperty StyleProperty = BindableProperty.Create("ThemeStyle", typeof(string), typeof(VisualElement), default(string));

		public static readonly BindableProperty IsFocusAllowedProperty = BindableProperty.Create("IsFocusAllowed", typeof(bool?), typeof(VisualElement), null);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty NextFocusDirectionProperty = BindableProperty.Create("NextFocusDirection", typeof(string), typeof(VisualElement), FocusDirection.None, propertyChanged: OnNextFocusDirectionPropertyChanged);

		public static readonly BindableProperty NextFocusUpViewProperty = BindableProperty.Create("NextFocusUpView", typeof(View), typeof(VisualElement), default(View));

		public static readonly BindableProperty NextFocusDownViewProperty = BindableProperty.Create("NextFocusDownView", typeof(View), typeof(VisualElement), default(View));

		public static readonly BindableProperty NextFocusLeftViewProperty = BindableProperty.Create("NextFocusLeftView", typeof(View), typeof(VisualElement), default(View));

		public static readonly BindableProperty NextFocusRightViewProperty = BindableProperty.Create("NextFocusRightView", typeof(View), typeof(VisualElement), default(View));

		public static readonly BindableProperty NextFocusBackViewProperty = BindableProperty.Create("NextFocusBackView", typeof(View), typeof(VisualElement), default(View));

		public static readonly BindableProperty NextFocusForwardViewProperty = BindableProperty.Create("NextFocusForwardView", typeof(View), typeof(VisualElement), default(View));

		public static readonly BindableProperty ToolTipProperty = BindableProperty.Create("ToolTip", typeof(string), typeof(VisualElement), default(string));

		public static string GetStyle(BindableObject element)
		{
			return (string)element.GetValue(StyleProperty);
		}

		public static void SetStyle(BindableObject element, string value)
		{
			element.SetValue(StyleProperty, value);
		}

		public static string GetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetStyle(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetStyle(config.Element, value);
			return config;
		}

		public static bool? IsFocusAllowed(BindableObject element)
		{
			return (bool?)element.GetValue(IsFocusAllowedProperty);
		}

		public static void SetFocusAllowed(BindableObject element, bool value)
		{
			element.SetValue(IsFocusAllowedProperty, value);
		}

		public static bool? IsFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return IsFocusAllowed(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetFocusAllowed(config.Element, value);
			return config;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(BindableObject element)
		{
			return (string)element.GetValue(NextFocusDirectionProperty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetNextFocusDirection(BindableObject element, string value)
		{
			element.SetValue(NextFocusDirectionProperty, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDirection(config.Element);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetNextFocusDirection(config.Element, value);
			return config;
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusUp(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Up);
			return config;
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusDown(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Down);
			return config;
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusLeft(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Left);
			return config;
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusRight(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Right);
			return config;
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusBack(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Back);
			return config;
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusForward(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Forward);
			return config;
		}

		public static View GetNextFocusUpView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusUpViewProperty);
		}

		public static void SetNextFocusUpView(BindableObject element, View value)
		{
			element.SetValue(NextFocusUpViewProperty, value);
		}

		public static View GetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusUpView(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusUpView(config.Element, value);
			return config;
		}

		public static View GetNextFocusDownView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusDownViewProperty);
		}

		public static void SetNextFocusDownView(BindableObject element, View value)
		{
			element.SetValue(NextFocusDownViewProperty, value);
		}

		public static View GetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDownView(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusDownView(config.Element, value);
			return config;
		}

		public static View GetNextFocusLeftView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusLeftViewProperty);
		}

		public static void SetNextFocusLeftView(BindableObject element, View value)
		{
			element.SetValue(NextFocusLeftViewProperty, value);
		}

		public static View GetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusLeftView(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusLeftView(config.Element, value);
			return config;
		}

		public static View GetNextFocusRightView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusRightViewProperty);
		}

		public static void SetNextFocusRightView(BindableObject element, View value)
		{
			element.SetValue(NextFocusRightViewProperty, value);
		}

		public static View GetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusRightView(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusRightView(config.Element, value);
			return config;
		}

		public static View GetNextFocusBackView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusBackViewProperty);
		}

		public static void SetNextFocusBackView(BindableObject element, View value)
		{
			element.SetValue(NextFocusBackViewProperty, value);
		}

		public static View GetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusBackView(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusBackView(config.Element, value);
			return config;
		}

		public static View GetNextFocusForwardView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusForwardViewProperty);
		}

		public static void SetNextFocusForwardView(BindableObject element, View value)
		{
			element.SetValue(NextFocusForwardViewProperty, value);
		}

		public static View GetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusForwardView(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusForwardView(config.Element, value);
			return config;
		}

		static void OnNextFocusDirectionPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			bindable.SetValue(NextFocusDirectionProperty, FocusDirection.None);
		}

		public static string GetToolTip(BindableObject element)
		{
			return (string)element.GetValue(ToolTipProperty);
		}

		public static void SetToolTip(BindableObject element, string value)
		{
			element.SetValue(ToolTipProperty, value);
		}

		public static string GetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetToolTip(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetToolTip(config.Element, value);
			return config;
		}
	}
}