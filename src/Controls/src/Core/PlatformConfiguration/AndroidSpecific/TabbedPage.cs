#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsElement = Maui.Controls.TabbedPage;

	/// <summary>The tabbed page instance that Microsoft.Maui.Controls created on the Android platform.</summary>
	public static class TabbedPage
	{
		/// <summary>Bindable property for <see cref="IsSwipePagingEnabled"/>.</summary>
		public static readonly BindableProperty IsSwipePagingEnabledProperty =
			BindableProperty.Create("IsSwipePagingEnabled", typeof(bool),
			typeof(TabbedPage), true);

		/// <summary>Returns a Boolean value that tells whether swiped paging is enabled.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether swipe paging is enabled.</returns>
		public static bool GetIsSwipePagingEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSwipePagingEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSwipePagingEnabled'][1]/Docs/*" />
		public static void SetIsSwipePagingEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSwipePagingEnabledProperty, value);
		}

		/// <summary>Gets a Boolean value that controls whether swipe paging is enabled.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns><see langword="true"/> if swiped paging is enabled. Otherwise, <see langword="false"/>.</returns>
		public static bool IsSwipePagingEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsSwipePagingEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSwipePagingEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsSwipePagingEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsSwipePagingEnabled(config.Element, value);
			return config;
		}

		/// <summary>Enables swiped paging.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated element on the Android platform.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> EnableSwipePaging(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSwipePagingEnabled(config.Element, true);
			return config;
		}

		/// <summary>Disables swiped paging.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated element on the Android platform.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> DisableSwipePaging(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSwipePagingEnabled(config.Element, false);
			return config;
		}

		/// <summary>Bindable property for <see cref="IsSmoothScrollEnabled"/>.</summary>
		public static readonly BindableProperty IsSmoothScrollEnabledProperty =
			BindableProperty.Create("IsSmoothScrollEnabled", typeof(bool),
			typeof(TabbedPage), true);

		/// <summary>Gets whether smooth scrolling is enabled for <paramref name="element"/>.</summary>
		/// <param name="element">The element parameter.</param>
		public static bool GetIsSmoothScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSmoothScrollEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSmoothScrollEnabled'][1]/Docs/*" />
		public static void SetIsSmoothScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSmoothScrollEnabledProperty, value);
		}

		/// <summary>Gets whether smooth scrolling is enabled for <c>this</c><see cref="Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage"/>.</summary>
		/// <param name="config">The config parameter.</param>
		public static bool IsSmoothScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsSmoothScrollEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetIsSmoothScrollEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsSmoothScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsSmoothScrollEnabled(config.Element, value);
			return config;
		}

		/// <summary>Turns on smooth scrolling for <c>this</c><see cref="Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage"/>.</summary>
		/// <param name="config">The config parameter.</param>
		public static IPlatformElementConfiguration<Android, FormsElement> EnableSmoothScroll(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSmoothScrollEnabled(config.Element, true);
			return config;
		}

		/// <summary>Turns off smooth scrolling for <c>this</c><see cref="Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage"/>.</summary>
		/// <param name="config">The config parameter.</param>
		public static IPlatformElementConfiguration<Android, FormsElement> DisableSmoothScroll(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSmoothScrollEnabled(config.Element, false);
			return config;
		}

		/// <summary>Bindable property for <see cref="OffscreenPageLimit"/>.</summary>
		[System.Obsolete("OffscreenPageLimitProperty is obsolete. This property will be removed in a future version.")]
		public static readonly BindableProperty OffscreenPageLimitProperty =
			BindableProperty.Create("OffscreenPageLimit", typeof(int),
			typeof(TabbedPage), 3, validateValue: (binding, value) => (int)value >= 0);

		/// <summary>Returns the number of offscreen pages are cached in memory.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>The number of offscreen pages are cached in memory.</returns>
		[System.Obsolete("GetOffscreenPageLimit is obsolete. This method will be removed in a future version.")]
		public static int GetOffscreenPageLimit(BindableObject element)
		{
			return (int)element.GetValue(OffscreenPageLimitProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetOffscreenPageLimit'][1]/Docs/*" />
		[System.Obsolete("SetOffscreenPageLimit is obsolete. This method will be removed in a future version.")]
		public static void SetOffscreenPageLimit(BindableObject element, int value)
		{
			element.SetValue(OffscreenPageLimitProperty, value);
		}

		/// <summary>Returns the number of offscreen pages are cached in memory.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The number of offscreen pages are cached in memory.</returns>
		[System.Obsolete("OffscreenPageLimit is obsolete. This method will be removed in a future version.")]
		public static int OffscreenPageLimit(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetOffscreenPageLimit(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetOffscreenPageLimit'][2]/Docs/*" />
		[System.Obsolete("SetOffscreenPageLimit is obsolete. This method will be removed in a future version.")]
		public static IPlatformElementConfiguration<Android, FormsElement> SetOffscreenPageLimit(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			SetOffscreenPageLimit(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="ToolbarPlacement"/>.</summary>
		public static readonly BindableProperty ToolbarPlacementProperty =
			BindableProperty.Create("ToolbarPlacement", typeof(ToolbarPlacement),
			typeof(TabbedPage), ToolbarPlacement.Top);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetToolbarPlacement'][1]/Docs/*" />
		public static ToolbarPlacement GetToolbarPlacement(BindableObject element)
		{
			return (ToolbarPlacement)element.GetValue(ToolbarPlacementProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetToolbarPlacement'][1]/Docs/*" />
		public static void SetToolbarPlacement(BindableObject element, ToolbarPlacement value)
		{
			if (element.IsSet(ToolbarPlacementProperty) && GetToolbarPlacement(element) != value)
			{
				throw new global::System.InvalidOperationException("Changing the tabs placement after it's been set is not supported.");
			}

			element.SetValue(ToolbarPlacementProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetToolbarPlacement'][2]/Docs/*" />
		public static ToolbarPlacement GetToolbarPlacement(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetToolbarPlacement(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='SetToolbarPlacement'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetToolbarPlacement(this IPlatformElementConfiguration<Android, FormsElement> config, ToolbarPlacement value)
		{
			SetToolbarPlacement(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetMaxItemCount'][1]/Docs/*" />
		public static int GetMaxItemCount(BindableObject element)
		{
			if (GetToolbarPlacement(element) == ToolbarPlacement.Bottom)
			{
				return 5;
			}

			return int.MaxValue;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/TabbedPage.xml" path="//Member[@MemberName='GetMaxItemCount'][2]/Docs/*" />
		public static int GetMaxItemCount(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetMaxItemCount(config.Element);
		}
	}
}
