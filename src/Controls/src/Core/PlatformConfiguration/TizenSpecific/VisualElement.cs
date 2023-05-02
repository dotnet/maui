#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.VisualElement;
	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement']/Docs/*" />
	public static class VisualElement
	{
		/// <summary>Bindable property for <see cref="Style"/>.</summary>
		public static readonly BindableProperty StyleProperty = BindableProperty.Create("ThemeStyle", typeof(string), typeof(VisualElement), default(string));

		/// <summary>Bindable property for attached property <c>IsFocusAllowed</c>.</summary>
		public static readonly BindableProperty IsFocusAllowedProperty = BindableProperty.Create("IsFocusAllowed", typeof(bool?), typeof(VisualElement), null);

		/// <summary>Bindable property for attached property <c>NextFocusDirection</c>.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty NextFocusDirectionProperty = BindableProperty.Create("NextFocusDirection", typeof(string), typeof(VisualElement), FocusDirection.None, propertyChanged: OnNextFocusDirectionPropertyChanged);

		/// <summary>Bindable property for attached property <c>NextFocusUpView</c>.</summary>
		public static readonly BindableProperty NextFocusUpViewProperty = BindableProperty.Create("NextFocusUpView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusDownView</c>.</summary>
		public static readonly BindableProperty NextFocusDownViewProperty = BindableProperty.Create("NextFocusDownView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusLeftView</c>.</summary>
		public static readonly BindableProperty NextFocusLeftViewProperty = BindableProperty.Create("NextFocusLeftView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusRightView</c>.</summary>
		public static readonly BindableProperty NextFocusRightViewProperty = BindableProperty.Create("NextFocusRightView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusBackView</c>.</summary>
		public static readonly BindableProperty NextFocusBackViewProperty = BindableProperty.Create("NextFocusBackView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusForwardView</c>.</summary>
		public static readonly BindableProperty NextFocusForwardViewProperty = BindableProperty.Create("NextFocusForwardView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for <see cref="ToolTip"/>.</summary>
		public static readonly BindableProperty ToolTipProperty = BindableProperty.Create("ToolTip", typeof(string), typeof(VisualElement), default(string));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetStyle'][1]/Docs/*" />
		public static string GetStyle(BindableObject element)
		{
			return (string)element.GetValue(StyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetStyle'][1]/Docs/*" />
		public static void SetStyle(BindableObject element, string value)
		{
			element.SetValue(StyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetStyle'][2]/Docs/*" />
		public static string GetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetStyle(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetStyle'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetStyle(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='IsFocusAllowed'][1]/Docs/*" />
		public static bool? IsFocusAllowed(BindableObject element)
		{
			return (bool?)element.GetValue(IsFocusAllowedProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetFocusAllowed'][1]/Docs/*" />
		public static void SetFocusAllowed(BindableObject element, bool value)
		{
			element.SetValue(IsFocusAllowedProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='IsFocusAllowed'][2]/Docs/*" />
		public static bool? IsFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return IsFocusAllowed(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetFocusAllowed'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetFocusAllowed(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDirection'][1]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(BindableObject element)
		{
			return (string)element.GetValue(NextFocusDirectionProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDirection'][1]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetNextFocusDirection(BindableObject element, string value)
		{
			element.SetValue(NextFocusDirectionProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDirection'][2]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDirection(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDirection'][2]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetNextFocusDirection(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusUp']/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusUp(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Up);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusDown']/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusDown(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Down);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusLeft']/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusLeft(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Left);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusRight']/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusRight(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Right);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusBack']/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusBack(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Back);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='MoveFocusForward']/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusForward(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Forward);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusUpView'][1]/Docs/*" />
		public static View GetNextFocusUpView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusUpViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusUpView'][1]/Docs/*" />
		public static void SetNextFocusUpView(BindableObject element, View value)
		{
			element.SetValue(NextFocusUpViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusUpView'][2]/Docs/*" />
		public static View GetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusUpView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusUpView'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusUpView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDownView'][1]/Docs/*" />
		public static View GetNextFocusDownView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusDownViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDownView'][1]/Docs/*" />
		public static void SetNextFocusDownView(BindableObject element, View value)
		{
			element.SetValue(NextFocusDownViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusDownView'][2]/Docs/*" />
		public static View GetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDownView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusDownView'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusDownView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusLeftView'][1]/Docs/*" />
		public static View GetNextFocusLeftView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusLeftViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusLeftView'][1]/Docs/*" />
		public static void SetNextFocusLeftView(BindableObject element, View value)
		{
			element.SetValue(NextFocusLeftViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusLeftView'][2]/Docs/*" />
		public static View GetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusLeftView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusLeftView'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusLeftView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusRightView'][1]/Docs/*" />
		public static View GetNextFocusRightView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusRightViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusRightView'][1]/Docs/*" />
		public static void SetNextFocusRightView(BindableObject element, View value)
		{
			element.SetValue(NextFocusRightViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusRightView'][2]/Docs/*" />
		public static View GetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusRightView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusRightView'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusRightView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusBackView'][1]/Docs/*" />
		public static View GetNextFocusBackView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusBackViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusBackView'][1]/Docs/*" />
		public static void SetNextFocusBackView(BindableObject element, View value)
		{
			element.SetValue(NextFocusBackViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusBackView'][2]/Docs/*" />
		public static View GetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusBackView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusBackView'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusBackView(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusForwardView'][1]/Docs/*" />
		public static View GetNextFocusForwardView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusForwardViewProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusForwardView'][1]/Docs/*" />
		public static void SetNextFocusForwardView(BindableObject element, View value)
		{
			element.SetValue(NextFocusForwardViewProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetNextFocusForwardView'][2]/Docs/*" />
		public static View GetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusForwardView(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetNextFocusForwardView'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusForwardView(config.Element, value);
			return config;
		}

		static void OnNextFocusDirectionPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			bindable.SetValue(NextFocusDirectionProperty, FocusDirection.None);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetToolTip'][1]/Docs/*" />
		public static string GetToolTip(BindableObject element)
		{
			return (string)element.GetValue(ToolTipProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetToolTip'][1]/Docs/*" />
		public static void SetToolTip(BindableObject element, string value)
		{
			element.SetValue(ToolTipProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='GetToolTip'][2]/Docs/*" />
		public static string GetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetToolTip(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/VisualElement.xml" path="//Member[@MemberName='SetToolTip'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetToolTip(config.Element, value);
			return config;
		}
	}
}
