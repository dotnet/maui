namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsElement = Maui.Controls.TabbedPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage']/Docs" />
	public static class TabbedPage
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='IsSwipePagingEnabledProperty']/Docs" />
		public static readonly BindableProperty IsSwipePagingEnabledProperty =
			BindableProperty.Create("IsSwipePagingEnabled", typeof(bool),
			typeof(TabbedPage), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetIsSwipePagingEnabled']/Docs" />
		public static bool GetIsSwipePagingEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSwipePagingEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSwipePagingEnabled'][1]/Docs" />
		public static void SetIsSwipePagingEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSwipePagingEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='IsSwipePagingEnabled']/Docs" />
		public static bool IsSwipePagingEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsSwipePagingEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSwipePagingEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsSwipePagingEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsSwipePagingEnabled(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='EnableSwipePaging']/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> EnableSwipePaging(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSwipePagingEnabled(config.Element, true);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='DisableSwipePaging']/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> DisableSwipePaging(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSwipePagingEnabled(config.Element, false);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='IsSmoothScrollEnabledProperty']/Docs" />
		public static readonly BindableProperty IsSmoothScrollEnabledProperty =
			BindableProperty.Create("IsSmoothScrollEnabled", typeof(bool),
			typeof(TabbedPage), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetIsSmoothScrollEnabled']/Docs" />
		public static bool GetIsSmoothScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSmoothScrollEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSmoothScrollEnabled'][1]/Docs" />
		public static void SetIsSmoothScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSmoothScrollEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='IsSmoothScrollEnabled']/Docs" />
		public static bool IsSmoothScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsSmoothScrollEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSmoothScrollEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsSmoothScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsSmoothScrollEnabled(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='EnableSmoothScroll']/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> EnableSmoothScroll(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSmoothScrollEnabled(config.Element, true);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='DisableSmoothScroll']/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> DisableSmoothScroll(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSmoothScrollEnabled(config.Element, false);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='OffscreenPageLimitProperty']/Docs" />
		public static readonly BindableProperty OffscreenPageLimitProperty =
			BindableProperty.Create("OffscreenPageLimit", typeof(int),
			typeof(TabbedPage), 3, validateValue: (binding, value) => (int)value >= 0);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetOffscreenPageLimit']/Docs" />
		public static int GetOffscreenPageLimit(BindableObject element)
		{
			return (int)element.GetValue(OffscreenPageLimitProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetOffscreenPageLimit'][1]/Docs" />
		public static void SetOffscreenPageLimit(BindableObject element, int value)
		{
			element.SetValue(OffscreenPageLimitProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='OffscreenPageLimit']/Docs" />
		public static int OffscreenPageLimit(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetOffscreenPageLimit(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetOffscreenPageLimit'][2]/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetOffscreenPageLimit(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			SetOffscreenPageLimit(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='ToolbarPlacementProperty']/Docs" />
		public static readonly BindableProperty ToolbarPlacementProperty =
			BindableProperty.Create("ToolbarPlacement", typeof(ToolbarPlacement),
			typeof(TabbedPage), ToolbarPlacement.Top);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetToolbarPlacement'][1]/Docs" />
		public static ToolbarPlacement GetToolbarPlacement(BindableObject element)
		{
			return (ToolbarPlacement)element.GetValue(ToolbarPlacementProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetToolbarPlacement'][1]/Docs" />
		public static void SetToolbarPlacement(BindableObject element, ToolbarPlacement value)
		{
			if (element.IsSet(ToolbarPlacementProperty) && GetToolbarPlacement(element) != value)
			{
				throw new global::System.InvalidOperationException("Changing the tabs placement after it's been set is not supported.");
			}

			element.SetValue(ToolbarPlacementProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetToolbarPlacement'][2]/Docs" />
		public static ToolbarPlacement GetToolbarPlacement(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetToolbarPlacement(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetToolbarPlacement'][2]/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetToolbarPlacement(this IPlatformElementConfiguration<Android, FormsElement> config, ToolbarPlacement value)
		{
			SetToolbarPlacement(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetMaxItemCount'][1]/Docs" />
		public static int GetMaxItemCount(BindableObject element)
		{
			if (GetToolbarPlacement(element) == ToolbarPlacement.Bottom)
			{
				return 5;
			}

			return int.MaxValue;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetMaxItemCount'][2]/Docs" />
		public static int GetMaxItemCount(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetMaxItemCount(config.Element);
		}
	}
}
