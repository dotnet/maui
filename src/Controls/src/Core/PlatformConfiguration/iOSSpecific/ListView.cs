#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.ListView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView']/Docs/*" />
	public static class ListView
	{
		/// <summary>Bindable property for <see cref="SeparatorStyle"/>.</summary>
#pragma warning disable CS0618 // Type or member is obsolete
		public static readonly BindableProperty SeparatorStyleProperty = BindableProperty.Create(nameof(SeparatorStyle), typeof(SeparatorStyle), typeof(FormsElement), SeparatorStyle.Default);
#pragma warning restore CS0618 // Type or member is obsolete

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='GetSeparatorStyle'][1]/Docs/*" />
		public static SeparatorStyle GetSeparatorStyle(BindableObject element)
		{
			return (SeparatorStyle)element.GetValue(SeparatorStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='SetSeparatorStyle'][1]/Docs/*" />
		public static void SetSeparatorStyle(BindableObject element, SeparatorStyle value)
		{
			element.SetValue(SeparatorStyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='GetSeparatorStyle'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public static SeparatorStyle GetSeparatorStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetSeparatorStyle(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='SetSeparatorStyle'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSeparatorStyle(this IPlatformElementConfiguration<iOS, FormsElement> config, SeparatorStyle value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetSeparatorStyle(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="GroupHeaderStyle"/>.</summary>
#pragma warning disable CS0618 // Type or member is obsolete
		public static readonly BindableProperty GroupHeaderStyleProperty = BindableProperty.Create(nameof(GroupHeaderStyle), typeof(GroupHeaderStyle), typeof(FormsElement), GroupHeaderStyle.Plain);
#pragma warning restore CS0618 // Type or member is obsolete

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='GetGroupHeaderStyle'][1]/Docs/*" />
		public static GroupHeaderStyle GetGroupHeaderStyle(BindableObject element)
		{
			return (GroupHeaderStyle)element.GetValue(GroupHeaderStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='SetGroupHeaderStyle'][1]/Docs/*" />
		public static void SetGroupHeaderStyle(BindableObject element, GroupHeaderStyle value)
		{
			element.SetValue(GroupHeaderStyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='GetGroupHeaderStyle'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public static GroupHeaderStyle GetGroupHeaderStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetGroupHeaderStyle(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='SetGroupHeaderStyle'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		public static IPlatformElementConfiguration<iOS, FormsElement> SetGroupHeaderStyle(this IPlatformElementConfiguration<iOS, FormsElement> config, GroupHeaderStyle value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetGroupHeaderStyle(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="RowAnimationsEnabled"/>.</summary>
		public static readonly BindableProperty RowAnimationsEnabledProperty = BindableProperty.Create(nameof(RowAnimationsEnabled), typeof(bool), typeof(ListView), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='GetRowAnimationsEnabled']/Docs/*" />
		public static bool GetRowAnimationsEnabled(BindableObject element)
		{
			return (bool)element.GetValue(RowAnimationsEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='SetRowAnimationsEnabled'][1]/Docs/*" />
		public static void SetRowAnimationsEnabled(BindableObject element, bool value)
		{
			element.SetValue(RowAnimationsEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='SetRowAnimationsEnabled'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		public static IPlatformElementConfiguration<iOS, FormsElement> SetRowAnimationsEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetRowAnimationsEnabled(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ListView.xml" path="//Member[@MemberName='RowAnimationsEnabled']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public static bool RowAnimationsEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetRowAnimationsEnabled(config.Element);
		}
	}
}
