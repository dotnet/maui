#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat
{
	using FormsElement = Maui.Controls.Application;

	/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat.Application']/Docs/*" />
	public static class Application
	{
		/// <summary>Bindable property for <see cref="SendDisappearingEventOnPause"/>.</summary>
		public static readonly BindableProperty SendDisappearingEventOnPauseProperty = BindableProperty.Create(nameof(SendDisappearingEventOnPause), typeof(bool), typeof(Application), true);

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='GetSendDisappearingEventOnPause'][1]/Docs/*" />
		public static bool GetSendDisappearingEventOnPause(BindableObject element)
		{
			return (bool)element.GetValue(SendDisappearingEventOnPauseProperty);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='SetSendDisappearingEventOnPause']/Docs/*" />
		public static void SetSendDisappearingEventOnPause(BindableObject element, bool value)
		{
			element.SetValue(SendDisappearingEventOnPauseProperty, value);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='GetSendDisappearingEventOnPause'][2]/Docs/*" />
		public static bool GetSendDisappearingEventOnPause(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSendDisappearingEventOnPause(config.Element);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='SendDisappearingEventOnPause']/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SendDisappearingEventOnPause(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetSendDisappearingEventOnPause(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="SendAppearingEventOnResume"/>.</summary>
		public static readonly BindableProperty SendAppearingEventOnResumeProperty = BindableProperty.Create(nameof(SendAppearingEventOnResume), typeof(bool), typeof(Application), true);

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='GetSendAppearingEventOnResume'][1]/Docs/*" />
		public static bool GetSendAppearingEventOnResume(BindableObject element)
		{
			return (bool)element.GetValue(SendAppearingEventOnResumeProperty);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='SetSendAppearingEventOnResume']/Docs/*" />
		public static void SetSendAppearingEventOnResume(BindableObject element, bool value)
		{
			element.SetValue(SendAppearingEventOnResumeProperty, value);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='GetSendAppearingEventOnResume'][2]/Docs/*" />
		public static bool GetSendAppearingEventOnResume(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSendAppearingEventOnResume(config.Element);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='SendAppearingEventOnResume']/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SendAppearingEventOnResume(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetSendAppearingEventOnResume(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="ShouldPreserveKeyboardOnResume"/>.</summary>
		public static readonly BindableProperty ShouldPreserveKeyboardOnResumeProperty = BindableProperty.Create(nameof(ShouldPreserveKeyboardOnResume), typeof(bool), typeof(Application), false);

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='GetShouldPreserveKeyboardOnResume'][1]/Docs/*" />
		public static bool GetShouldPreserveKeyboardOnResume(BindableObject element)
		{
			return (bool)element.GetValue(ShouldPreserveKeyboardOnResumeProperty);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='SetShouldPreserveKeyboardOnResume']/Docs/*" />
		public static void SetShouldPreserveKeyboardOnResume(BindableObject element, bool value)
		{
			element.SetValue(ShouldPreserveKeyboardOnResumeProperty, value);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='GetShouldPreserveKeyboardOnResume'][2]/Docs/*" />
		public static bool GetShouldPreserveKeyboardOnResume(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetShouldPreserveKeyboardOnResume(config.Element);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/Application.xml" path="//Member[@MemberName='ShouldPreserveKeyboardOnResume']/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> ShouldPreserveKeyboardOnResume(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetShouldPreserveKeyboardOnResume(config.Element, value);
			return config;
		}
	}
}
