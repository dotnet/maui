#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.ProgressBar;

	/// <summary>Provides access to the pulsing status for progress bars.</summary>
	public static class ProgressBar
	{
		/// <summary>Bindable property for attached property <c>ProgressBarPulsingStatus</c>.</summary>
		public static readonly BindableProperty ProgressBarPulsingStatusProperty =
			BindableProperty.Create("ProgressBarPulsingStatus", typeof(bool),
			typeof(FormsElement), false);

		/// <summary>Gets the pulsing status for the progress bar.</summary>
		/// <param name="element">The progress bar element whose pulsing status to get.</param>
		/// <returns><see langword="true"/> if the progress bar is pulsing; otherwise, <see langword="false"/>.</returns>
		public static bool GetPulsingStatus(BindableObject element)
		{
			return (bool)element.GetValue(ProgressBarPulsingStatusProperty);
		}

		/// <summary>Sets the pulsing status for the progress bar. Only applies when style is <see cref="ProgressBarStyle.Pending"/>.</summary>
		/// <param name="element">The progress bar element whose pulsing status to set.</param>
		/// <param name="isPulsing"><see langword="true"/> to enable pulsing; otherwise, <see langword="false"/>.</param>
		public static void SetPulsingStatus(BindableObject element, bool isPulsing)
		{
			string style = VisualElement.GetStyle(element);
			if (style == ProgressBarStyle.Pending)
			{
				element.SetValue(ProgressBarPulsingStatusProperty, isPulsing);
			}
		}

		/// <summary>Gets the pulsing status for the progress bar.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if the progress bar is pulsing; otherwise, <see langword="false"/>.</returns>
		public static bool GetPulsingStatus(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetPulsingStatus(config.Element);
		}

		/// <summary>Sets the pulsing status for the progress bar. Only applies when style is <see cref="ProgressBarStyle.Pending"/>.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="isPulsing"><see langword="true"/> to enable pulsing; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetPulsingStatus(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool isPulsing)
		{
			SetPulsingStatus(config.Element, isPulsing);
			return config;
		}
	}
}
