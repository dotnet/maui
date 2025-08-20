#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.ListView;

	/// <summary>The list view instance that Microsoft.Maui.Controls created on the Android platform.</summary>
	public static class ListView
	{
		/// <summary>Bindable property for <see cref="IsFastScrollEnabled"/>.</summary>
		public static readonly BindableProperty IsFastScrollEnabledProperty = BindableProperty.Create("IsFastScrollEnabled", typeof(bool), typeof(ListView), false);

		/// <summary>Returns a Boolean value that tells whether fast scrolling is enabled.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether fast scrolling is enabled.</returns>
		public static bool GetIsFastScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsFastScrollEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][1]/Docs/*" />
		public static void SetIsFastScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsFastScrollEnabledProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether fast scrolling is enabled.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether fast scrolling is enabled.</returns>
		public static bool IsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsFastScrollEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsFastScrollEnabled(config.Element, value);
			return config;
		}
	}
}
