#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsElement = Maui.Controls.ListView;

	/// <summary>The list view instance that Microsoft.Maui.Controls created on the Android platform.</summary>
	[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
	public static class ListView
	{
		/// <summary>Bindable property for <see cref="IsFastScrollEnabled"/>.</summary>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty IsFastScrollEnabledProperty = BindableProperty.Create("IsFastScrollEnabled", typeof(bool), typeof(ListView), false);

		/// <summary>Returns a Boolean value that tells whether fast scrolling is enabled.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether fast scrolling is enabled.</returns>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static bool GetIsFastScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsFastScrollEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][1]/Docs/*" />
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static void SetIsFastScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsFastScrollEnabledProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether fast scrolling is enabled.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether fast scrolling is enabled.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static bool IsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return GetIsFastScrollEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetIsFastScrollEnabled(config.Element, value);
			return config;
		}
	}
}
