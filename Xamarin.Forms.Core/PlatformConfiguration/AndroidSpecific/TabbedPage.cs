namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using System;
	using System.ComponentModel;
	using FormsElement = Forms.TabbedPage;

	public static class TabbedPage
	{
		public static readonly BindableProperty IsSwipePagingEnabledProperty =
			BindableProperty.Create("IsSwipePagingEnabled", typeof(bool),
			typeof(TabbedPage), true);

		public static bool GetIsSwipePagingEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSwipePagingEnabledProperty);
		}

		public static void SetIsSwipePagingEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSwipePagingEnabledProperty, value);
		}

		public static bool IsSwipePagingEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsSwipePagingEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetIsSwipePagingEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsSwipePagingEnabled(config.Element, value);
			return config;
		}

		public static IPlatformElementConfiguration<Android, FormsElement> EnableSwipePaging(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSwipePagingEnabled(config.Element, true);
			return config;
		}

		public static IPlatformElementConfiguration<Android, FormsElement> DisableSwipePaging(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSwipePagingEnabled(config.Element, false);
			return config;
		}

		public static readonly BindableProperty IsSmoothScrollEnabledProperty =
			BindableProperty.Create("IsSmoothScrollEnabled", typeof(bool),
			typeof(TabbedPage), true);

		public static bool GetIsSmoothScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSmoothScrollEnabledProperty);
		}

		public static void SetIsSmoothScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSmoothScrollEnabledProperty, value);
		}

		public static bool IsSmoothScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsSmoothScrollEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetIsSmoothScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsSmoothScrollEnabled(config.Element, value);
			return config;
		}

		public static IPlatformElementConfiguration<Android, FormsElement> EnableSmoothScroll(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSmoothScrollEnabled(config.Element, true);
			return config;
		}

		public static IPlatformElementConfiguration<Android, FormsElement> DisableSmoothScroll(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			SetIsSmoothScrollEnabled(config.Element, false);
			return config;
		}

		public static readonly BindableProperty OffscreenPageLimitProperty =
			BindableProperty.Create("OffscreenPageLimit", typeof(int),
			typeof(TabbedPage), 3, validateValue: (binding, value) => (int)value >= 0);

		public static int GetOffscreenPageLimit(BindableObject element)
		{
			return (int)element.GetValue(OffscreenPageLimitProperty);
		}

		public static void SetOffscreenPageLimit(BindableObject element, int value)
		{
			element.SetValue(OffscreenPageLimitProperty, value);
		}

		public static int OffscreenPageLimit(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetOffscreenPageLimit(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetOffscreenPageLimit(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			SetOffscreenPageLimit(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ToolbarPlacementProperty =
			BindableProperty.Create("ToolbarPlacement", typeof(ToolbarPlacement),
			typeof(TabbedPage), ToolbarPlacement.Top);


		public static ToolbarPlacement GetToolbarPlacement(BindableObject element)
		{
			return (ToolbarPlacement)element.GetValue(ToolbarPlacementProperty);
		}

		public static void SetToolbarPlacement(BindableObject element, ToolbarPlacement value)
		{
			if (element.IsSet(ToolbarPlacementProperty) && GetToolbarPlacement(element) != value)
			{
				throw new InvalidOperationException("Changing the tabs placement after it's been set is not supported.");
			}

			element.SetValue(ToolbarPlacementProperty, value);
		}

		public static ToolbarPlacement GetToolbarPlacement(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetToolbarPlacement(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetToolbarPlacement(this IPlatformElementConfiguration<Android, FormsElement> config, ToolbarPlacement value)
		{
			SetToolbarPlacement(config.Element, value);
			return config;
		}

		public static int GetMaxItemCount(BindableObject element)
		{
			if (GetToolbarPlacement(element) == ToolbarPlacement.Bottom)
			{
				return 5;
			}

			return int.MaxValue;
		}

		public static int GetMaxItemCount(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetMaxItemCount(config.Element);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarItemColor is obsolete as of version 4.0. Please use TabbedPage.UnselectedTabColor instead.")]
		public static readonly BindableProperty BarItemColorProperty =
			BindableProperty.Create("BarItemColor", typeof(Color),
			typeof(TabbedPage), Color.Default, propertyChanged: (sender, oldValue, newValue) => { ((FormsElement)sender).UnselectedTabColor = (Color)newValue; });

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarItemColor is obsolete as of version 4.0. Please use TabbedPage.UnselectedTabColor instead.")]
		public static Color GetBarItemColor(BindableObject element)
		{
			return (Color)element.GetValue(BarItemColorProperty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarItemColor is obsolete as of version 4.0. Please use TabbedPage.UnselectedTabColor instead.")]
		public static void SetBarItemColor(BindableObject element, Color value)
		{
			element.SetValue(BarItemColorProperty, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarItemColor is obsolete as of version 4.0. Please use TabbedPage.UnselectedTabColor instead.")]
		public static Color GetBarItemColor(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetBarItemColor(config.Element);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarItemColor is obsolete as of version 4.0. Please use TabbedPage.UnselectedTabColor instead.")]
		public static IPlatformElementConfiguration<Android, FormsElement> SetBarItemColor(this IPlatformElementConfiguration<Android, FormsElement> config, Color value)
		{
			SetBarItemColor(config.Element, value);
			return config;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarSelectedItemColor is obsolete as of version 4.0. Please use TabbedPage.SelectedTabColor instead.")]
		public static readonly BindableProperty BarSelectedItemColorProperty =
			BindableProperty.Create("BarSelectedItemColor", typeof(Color),
			typeof(TabbedPage), Color.Default, propertyChanged: (sender, oldValue, newValue) => { ((FormsElement)sender).SelectedTabColor = (Color)newValue; });

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarSelectedItemColor is obsolete as of version 4.0. Please use TabbedPage.SelectedTabColor instead.")]
		public static Color GetBarSelectedItemColor(BindableObject element)
		{
			return (Color)element.GetValue(BarSelectedItemColorProperty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarSelectedItemColor is obsolete as of version 4.0. Please use TabbedPage.SelectedTabColor instead.")]
		public static void SetBarSelectedItemColor(BindableObject element, Color value)
		{
			element.SetValue(BarSelectedItemColorProperty, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarSelectedItemColor is obsolete as of version 4.0. Please use TabbedPage.SelectedTabColor instead.")]
		public static IPlatformElementConfiguration<Android, FormsElement> SetBarSelectedItemColor(this IPlatformElementConfiguration<Android, FormsElement> config, Color value)
		{
			SetBarSelectedItemColor(config.Element, value);
			return config;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("BarSelectedItemColor is obsolete as of version 4.0. Please use TabbedPage.SelectedTabColor instead.")]
		public static Color GetBarSelectedItemColor(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetBarSelectedItemColor(config.Element);
		}
	}
}