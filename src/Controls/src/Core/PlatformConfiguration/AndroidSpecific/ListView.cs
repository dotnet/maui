#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.ListView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView']/Docs/*" />
	public static class ListView
	{
		/// <summary>Bindable property for <see cref="IsFastScrollEnabled"/>.</summary>
		public static readonly BindableProperty IsFastScrollEnabledProperty = BindableProperty.Create("IsFastScrollEnabled", typeof(bool), typeof(ListView), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='GetIsFastScrollEnabled']/Docs/*" />
		public static bool GetIsFastScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsFastScrollEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][1]/Docs/*" />
		public static void SetIsFastScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsFastScrollEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='IsFastScrollEnabled']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public static bool IsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetIsFastScrollEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetIsFastScrollEnabled(config.Element, value);
			return config;
		}
	}
}
