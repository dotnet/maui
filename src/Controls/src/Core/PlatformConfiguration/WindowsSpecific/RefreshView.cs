#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.RefreshView;

	/// <summary>Provides Windows-specific configuration for the pull-to-refresh gesture direction.</summary>
	public static class RefreshView
	{
		public enum RefreshPullDirection
		{
			LeftToRight,
			TopToBottom,
			RightToLeft,
			BottomToTop
		}

		/// <summary>Bindable property for <see cref="RefreshPullDirection"/>.</summary>
		public static readonly BindableProperty RefreshPullDirectionProperty = BindableProperty.Create("RefreshPullDirection", typeof(RefreshPullDirection), typeof(FormsElement), RefreshPullDirection.TopToBottom);

		/// <summary>Sets the pull direction for the refresh gesture on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value">The pull direction.</param>
		public static void SetRefreshPullDirection(BindableObject element, RefreshPullDirection value)
		{
			element.SetValue(RefreshPullDirectionProperty, value);
		}

		/// <summary>Gets the pull direction for the refresh gesture on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The current pull direction.</returns>
		public static RefreshPullDirection GetRefreshPullDirection(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetRefreshPullDirection(config.Element);
		}

		/// <summary>Gets the pull direction for the refresh gesture on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns>The current pull direction.</returns>
		public static RefreshPullDirection GetRefreshPullDirection(BindableObject element)
		{
			return (RefreshPullDirection)element.GetValue(RefreshPullDirectionProperty);
		}

		/// <summary>Sets the pull direction for the refresh gesture on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The pull direction.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetRefreshPullDirection(
			this IPlatformElementConfiguration<Windows, FormsElement> config, RefreshPullDirection value)
		{
			SetRefreshPullDirection(config.Element, value);
			return config;
		}
	}

}
