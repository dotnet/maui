#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.Application;

	/// <summary>Enumerates values that control how an on-screen input interface is visually accommodated. This is a bindable property.</summary>
	public enum WindowSoftInputModeAdjust
	{
		/// <summary>Indicates that the content of the control will pan, possibly off of the screen, to accommodate the input interface.</summary>
		Pan,
		/// <summary>Indicates that the content of the control will resize to accommodate the input interface.</summary>
		Resize,
		/// <summary>Indicates that the behavior of the control for oversized content is not specified.</summary>
		Unspecified
	}

	/// <summary>The application instance that Microsoft.Maui.Controls created on the Android platform.</summary>
	public static class Application
	{
		/// <summary>Bindable property for <see cref="WindowSoftInputModeAdjust"/>.</summary>
		public static readonly BindableProperty WindowSoftInputModeAdjustProperty =
			BindableProperty.Create("WindowSoftInputModeAdjust", typeof(WindowSoftInputModeAdjust),
			typeof(Application), WindowSoftInputModeAdjust.Pan);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='GetWindowSoftInputModeAdjust'][1]/Docs/*" />
		public static WindowSoftInputModeAdjust GetWindowSoftInputModeAdjust(BindableObject element)
		{
			return (WindowSoftInputModeAdjust)element.GetValue(WindowSoftInputModeAdjustProperty);
		}

		/// <summary>Sets a value that controls whether the soft input mode of the provided <paramref name="element"/> pans or resizes its content to allow the display of the on-screen input UI.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		public static void SetWindowSoftInputModeAdjust(BindableObject element, WindowSoftInputModeAdjust value)
		{
			element.SetValue(WindowSoftInputModeAdjustProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='GetWindowSoftInputModeAdjust'][2]/Docs/*" />
		public static WindowSoftInputModeAdjust GetWindowSoftInputModeAdjust(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetWindowSoftInputModeAdjust(config.Element);
		}

		/// <summary>Sets a value that controls whether the soft input mode of the provided platform configuration pans or resizes its content to allow the display of the on-screen input UI.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		/// <returns>A value that controls whether the soft input mode of the provided platform configuration pans or resizes its content to allow the display of the on-screen input UI.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> UseWindowSoftInputModeAdjust(this IPlatformElementConfiguration<Android, FormsElement> config, WindowSoftInputModeAdjust value)
		{
			SetWindowSoftInputModeAdjust(config.Element, value);
			return config;
		}
	}
}
