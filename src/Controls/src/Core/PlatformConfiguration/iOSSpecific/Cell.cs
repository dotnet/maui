#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using System;
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Cell;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Cell']/Docs/*" />
	[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
	public static class Cell
	{
		/// <summary>Bindable property for <see cref="DefaultBackgroundColor"/>.</summary>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty DefaultBackgroundColorProperty = BindableProperty.Create(nameof(DefaultBackgroundColor), typeof(Color), typeof(Cell), null);

		/// <param name="element">The element parameter.</param>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static Color GetDefaultBackgroundColor(BindableObject element)
			=> (Color)element.GetValue(DefaultBackgroundColorProperty);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][1]/Docs/*" />
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static void SetDefaultBackgroundColor(BindableObject element, Color value)
			=> element.SetValue(DefaultBackgroundColorProperty, value);

		/// <param name="config">The config parameter.</param>
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static Color DefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
			=> GetDefaultBackgroundColor(config.Element);

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetDefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetDefaultBackgroundColor(config.Element, value);
			return config;
		}
	}
}
