#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsCell = Maui.Controls.Cell;

	/// <summary>Android-specific context actions behavior for ViewCell in ListView.</summary>
	[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
	public static class ViewCell
	{
		/// <summary>Bindable property for attached property <c>IsContextActionsLegacyModeEnabled</c>.</summary>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty IsContextActionsLegacyModeEnabledProperty = BindableProperty.Create("IsContextActionsLegacyModeEnabled", typeof(bool), typeof(Maui.Controls.ViewCell), false, propertyChanged: OnIsContextActionsLegacyModeEnabledPropertyChanged);
#pragma warning restore CS0618 // Type or member is obsolete

		private static void OnIsContextActionsLegacyModeEnabledPropertyChanged(BindableObject element, object oldValue, object newValue)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = element as FormsCell;
#pragma warning restore CS0618 // Type or member is obsolete
			cell.IsContextActionsLegacyModeEnabled = (bool)newValue;
		}

		/// <summary>Gets whether the legacy context actions mode is enabled on Android.</summary>
		/// <param name="element">The cell element.</param>
		/// <returns><see langword="true"/> if legacy mode is enabled; otherwise, <see langword="false"/>.</returns>
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static bool GetIsContextActionsLegacyModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsContextActionsLegacyModeEnabledProperty);
		}

		/// <summary>Sets whether the legacy context actions mode is enabled on Android.</summary>
		/// <param name="element">The cell element.</param>
		/// <param name="value"><see langword="true"/> to enable legacy mode; otherwise, <see langword="false"/>.</param>
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static void SetIsContextActionsLegacyModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsContextActionsLegacyModeEnabledProperty, value);
		}

		/// <summary>Gets whether the legacy context actions mode is enabled on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if legacy mode is enabled; otherwise, <see langword="false"/>.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static bool GetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetIsContextActionsLegacyModeEnabled(config.Element);
		}

		/// <summary>Sets whether the legacy context actions mode is enabled on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable legacy mode; otherwise, <see langword="false"/>.</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<Android, FormsCell> SetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config, bool value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetIsContextActionsLegacyModeEnabled(config.Element, value);
			return config;
		}
	}
}
