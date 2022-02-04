using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.VisualElement;
	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement']/Docs" />
	public static class VisualElement
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='StyleProperty']/Docs" />
		public static readonly BindableProperty StyleProperty = BindableProperty.Create("ThemeStyle", typeof(string), typeof(VisualElement), default(string));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='IsFocusAllowedProperty']/Docs" />
		public static readonly BindableProperty IsFocusAllowedProperty = BindableProperty.Create("IsFocusAllowed", typeof(bool?), typeof(VisualElement), null);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusDirectionProperty']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty NextFocusDirectionProperty = BindableProperty.Create("NextFocusDirection", typeof(string), typeof(VisualElement), FocusDirection.None, propertyChanged: OnNextFocusDirectionPropertyChanged);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusUpViewProperty']/Docs" />
		public static readonly BindableProperty NextFocusUpViewProperty = BindableProperty.Create("NextFocusUpView", typeof(View), typeof(VisualElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusDownViewProperty']/Docs" />
		public static readonly BindableProperty NextFocusDownViewProperty = BindableProperty.Create("NextFocusDownView", typeof(View), typeof(VisualElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusLeftViewProperty']/Docs" />
		public static readonly BindableProperty NextFocusLeftViewProperty = BindableProperty.Create("NextFocusLeftView", typeof(View), typeof(VisualElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusRightViewProperty']/Docs" />
		public static readonly BindableProperty NextFocusRightViewProperty = BindableProperty.Create("NextFocusRightView", typeof(View), typeof(VisualElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusBackViewProperty']/Docs" />
		public static readonly BindableProperty NextFocusBackViewProperty = BindableProperty.Create("NextFocusBackView", typeof(View), typeof(VisualElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='NextFocusForwardViewProperty']/Docs" />
		public static readonly BindableProperty NextFocusForwardViewProperty = BindableProperty.Create("NextFocusForwardView", typeof(View), typeof(VisualElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='ToolTipProperty']/Docs" />
		public static readonly BindableProperty ToolTipProperty = BindableProperty.Create("ToolTip", typeof(string), typeof(VisualElement), default(string));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetStyle'][0]/Docs" />
		public static string GetStyle(BindableObject element)
		{
			return (string)element.GetValue(StyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetStyle'][0]/Docs" />
		public static void SetStyle(BindableObject element, string value)
		{
			element.SetValue(StyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetStyle']/Docs" />
		public static string GetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetStyle(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetStyle']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetStyle(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='IsFocusAllowed'][0]/Docs" />
		public static bool? IsFocusAllowed(BindableObject element)
		{
			return (bool?)element.GetValue(IsFocusAllowedProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetFocusAllowed'][0]/Docs" />
		public static void SetFocusAllowed(BindableObject element, bool value)
		{
			element.SetValue(IsFocusAllowedProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='IsFocusAllowed']/Docs" />
		public static bool? IsFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return IsFocusAllowed(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetFocusAllowed']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetFocusAllowed(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDirection'][0]/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(BindableObject element)
		{
			return (string)element.GetValue(NextFocusDirectionProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDirection'][0]/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetNextFocusDirection(BindableObject element, string value)
		{
			element.SetValue(NextFocusDirectionProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDirection']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDirection(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDirection']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetNextFocusDirection(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusUp']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusUp(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Up);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusDown']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusDown(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Down);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusLeft']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusLeft(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Left);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusRight']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusRight(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Right);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusBack']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusBack(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Back);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusForward']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusForward(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Forward);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusUpView'][0]/Docs" />
		public static View GetNextFocusUpView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusUpViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusUpView'][0]/Docs" />
		public static void SetNextFocusUpView(BindableObject element, View value)
		{
			element.SetValue(NextFocusUpViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusUpView']/Docs" />
		public static View GetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusUpView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusUpView']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusUpView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDownView'][0]/Docs" />
		public static View GetNextFocusDownView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusDownViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDownView'][0]/Docs" />
		public static void SetNextFocusDownView(BindableObject element, View value)
		{
			element.SetValue(NextFocusDownViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDownView']/Docs" />
		public static View GetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDownView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDownView']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusDownView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusLeftView'][0]/Docs" />
		public static View GetNextFocusLeftView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusLeftViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusLeftView'][0]/Docs" />
		public static void SetNextFocusLeftView(BindableObject element, View value)
		{
			element.SetValue(NextFocusLeftViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusLeftView']/Docs" />
		public static View GetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusLeftView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusLeftView']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusLeftView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusRightView'][0]/Docs" />
		public static View GetNextFocusRightView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusRightViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusRightView'][0]/Docs" />
		public static void SetNextFocusRightView(BindableObject element, View value)
		{
			element.SetValue(NextFocusRightViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusRightView']/Docs" />
		public static View GetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusRightView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusRightView']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusRightView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusBackView'][0]/Docs" />
		public static View GetNextFocusBackView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusBackViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusBackView'][0]/Docs" />
		public static void SetNextFocusBackView(BindableObject element, View value)
		{
			element.SetValue(NextFocusBackViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusBackView']/Docs" />
		public static View GetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusBackView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusBackView']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusBackView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusForwardView'][0]/Docs" />
		public static View GetNextFocusForwardView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusForwardViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusForwardView'][0]/Docs" />
		public static void SetNextFocusForwardView(BindableObject element, View value)
		{
			element.SetValue(NextFocusForwardViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusForwardView']/Docs" />
		public static View GetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusForwardView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusForwardView']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusForwardView(config.Element, value);
			return config;
		}

		static void OnNextFocusDirectionPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			bindable.SetValue(NextFocusDirectionProperty, FocusDirection.None);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetToolTip'][0]/Docs" />
		public static string GetToolTip(BindableObject element)
		{
			return (string)element.GetValue(ToolTipProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetToolTip'][0]/Docs" />
		public static void SetToolTip(BindableObject element, string value)
		{
			element.SetValue(ToolTipProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetToolTip']/Docs" />
		public static string GetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetToolTip(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetToolTip']/Docs" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetToolTip(config.Element, value);
			return config;
		}
	}
}