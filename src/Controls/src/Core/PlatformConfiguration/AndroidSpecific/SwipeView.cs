namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.SwipeView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/SwipeView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.SwipeView']/Docs" />
	public static class SwipeView
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/SwipeView.xml" path="//Member[@MemberName='SwipeTransitionModeProperty']/Docs" />
		public static readonly BindableProperty SwipeTransitionModeProperty = BindableProperty.Create("SwipeTransitionMode", typeof(SwipeTransitionMode), typeof(SwipeView), SwipeTransitionMode.Reveal);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/SwipeView.xml" path="//Member[@MemberName='GetSwipeTransitionMode'][0]/Docs" />
		public static SwipeTransitionMode GetSwipeTransitionMode(BindableObject element)
		{
			return (SwipeTransitionMode)element.GetValue(SwipeTransitionModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/SwipeView.xml" path="//Member[@MemberName='SetSwipeTransitionMode'][0]/Docs" />
		public static void SetSwipeTransitionMode(BindableObject element, SwipeTransitionMode value)
		{
			element.SetValue(SwipeTransitionModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/SwipeView.xml" path="//Member[@MemberName='GetSwipeTransitionMode']/Docs" />
		public static SwipeTransitionMode GetSwipeTransitionMode(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSwipeTransitionMode(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/SwipeView.xml" path="//Member[@MemberName='SetSwipeTransitionMode']/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetSwipeTransitionMode(this IPlatformElementConfiguration<Android, FormsElement> config, SwipeTransitionMode value)
		{
			SetSwipeTransitionMode(config.Element, value);
			return config;
		}
	}
}