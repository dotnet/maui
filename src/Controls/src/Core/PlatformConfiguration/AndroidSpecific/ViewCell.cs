#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsCell = Maui.Controls.Cell;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ViewCell']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='GetIsContextActionsLegacyModeEnabled'][1]/Docs/*" />
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static bool GetIsContextActionsLegacyModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsContextActionsLegacyModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='SetIsContextActionsLegacyModeEnabled'][1]/Docs/*" />
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static void SetIsContextActionsLegacyModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsContextActionsLegacyModeEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='GetIsContextActionsLegacyModeEnabled'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
		public static bool GetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetIsContextActionsLegacyModeEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='SetIsContextActionsLegacyModeEnabled'][2]/Docs/*" />
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
