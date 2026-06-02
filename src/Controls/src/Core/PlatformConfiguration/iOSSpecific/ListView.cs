#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using System;
	using FormsElement = Maui.Controls.ListView;
	/// <summary>Provides access to the separator style for list views on the iOS platform.</summary>
	[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
	public static class ListView
	{
		/// <summary>Bindable property for <see cref="SeparatorStyle"/>.</summary>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty SeparatorStyleProperty = BindableProperty.Create(nameof(SeparatorStyle), typeof(SeparatorStyle), typeof(FormsElement), SeparatorStyle.Default);
#pragma warning restore CS0618 // Type or member is obsolete

		/// <summary>Gets the separator style for the ListView on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The separator style.</returns>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static SeparatorStyle GetSeparatorStyle(BindableObject element)
		{
			return (SeparatorStyle)element.GetValue(SeparatorStyleProperty);
		}

		/// <summary>Sets the separator style for the ListView on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">The separator style to apply.</param>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static void SetSeparatorStyle(BindableObject element, SeparatorStyle value)
		{
			element.SetValue(SeparatorStyleProperty, value);
		}

		/// <summary>Gets the separator style for the ListView on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The separator style.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static SeparatorStyle GetSeparatorStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetSeparatorStyle(config.Element);
		}

		/// <summary>Sets the separator style for the ListView on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The separator style to apply.</param>
		/// <returns>The updated platform configuration.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSeparatorStyle(this IPlatformElementConfiguration<iOS, FormsElement> config, SeparatorStyle value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetSeparatorStyle(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="GroupHeaderStyle"/>.</summary>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty GroupHeaderStyleProperty = BindableProperty.Create(nameof(GroupHeaderStyle), typeof(GroupHeaderStyle), typeof(FormsElement), GroupHeaderStyle.Plain);
#pragma warning restore CS0618 // Type or member is obsolete

		/// <summary>Gets the group header style for the ListView on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The group header style.</returns>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static GroupHeaderStyle GetGroupHeaderStyle(BindableObject element)
		{
			return (GroupHeaderStyle)element.GetValue(GroupHeaderStyleProperty);
		}

		/// <summary>Sets the group header style for the ListView on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">The group header style to apply.</param>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static void SetGroupHeaderStyle(BindableObject element, GroupHeaderStyle value)
		{
			element.SetValue(GroupHeaderStyleProperty, value);
		}

		/// <summary>Gets the group header style for the ListView on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The group header style.</returns>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
#pragma warning disable CS0618 // Type or member is obsolete
		public static GroupHeaderStyle GetGroupHeaderStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetGroupHeaderStyle(config.Element);
		}

		/// <summary>Sets the group header style for the ListView on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The group header style to apply.</param>
		/// <returns>The updated platform configuration.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetGroupHeaderStyle(this IPlatformElementConfiguration<iOS, FormsElement> config, GroupHeaderStyle value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetGroupHeaderStyle(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="RowAnimationsEnabled"/>.</summary>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty RowAnimationsEnabledProperty = BindableProperty.Create(nameof(RowAnimationsEnabled), typeof(bool), typeof(ListView), true);

		/// <param name="element">The element parameter.</param>	
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static bool GetRowAnimationsEnabled(BindableObject element)
		{
			return (bool)element.GetValue(RowAnimationsEnabledProperty);
		}

		/// <summary>Sets whether row animations are enabled for the ListView on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to enable animations; otherwise, <see langword="false"/>.</param>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static void SetRowAnimationsEnabled(BindableObject element, bool value)
		{
			element.SetValue(RowAnimationsEnabledProperty, value);
		}

		/// <summary>Sets whether row animations are enabled for the ListView on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable animations; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetRowAnimationsEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetRowAnimationsEnabled(config.Element, value);
			return config;
		}

		/// <param name="config">The config parameter.</param>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
				public static bool RowAnimationsEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetRowAnimationsEnabled(config.Element);
		}
	}
}
