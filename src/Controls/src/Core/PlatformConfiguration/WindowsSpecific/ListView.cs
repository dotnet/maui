#nullable disable
using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.ListView;

	/// <summary>Platform-specific properties for list view controls on UWP.</summary>
	[Obsolete("With the deprecation of ListView, this class is obsolete. Please use CollectionView instead.")]
	public static class ListView
	{
		#region SelectionMode

		/// <summary>Bindable property for <see cref="SelectionMode"/>.</summary>
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static readonly BindableProperty SelectionModeProperty =
			BindableProperty.CreateAttached("WindowsSelectionMode", typeof(ListViewSelectionMode),
				typeof(ListView), ListViewSelectionMode.Accessible);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='GetSelectionMode'][1]/Docs/*" />
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static ListViewSelectionMode GetSelectionMode(BindableObject element)
		{
			return (ListViewSelectionMode)element.GetValue(SelectionModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='SetSelectionMode'][1]/Docs/*" />
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static void SetSelectionMode(BindableObject element, ListViewSelectionMode value)
		{
			element.SetValue(SelectionModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='GetSelectionMode'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static ListViewSelectionMode GetSelectionMode(this IPlatformElementConfiguration<Windows, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return (ListViewSelectionMode)config.Element.GetValue(SelectionModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='SetSelectionMode'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("With the deprecation of ListView, this property is obsolete. Please use CollectionView instead.")]
		public static IPlatformElementConfiguration<Windows, FormsElement> SetSelectionMode(
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			this IPlatformElementConfiguration<Windows, FormsElement> config, ListViewSelectionMode value)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			config.Element.SetValue(SelectionModeProperty, value);
			return config;
		}

		#endregion
	}

	/// <summary>Selection modes for list view controls on UWP.</summary>
	[Obsolete("With the deprecation of ListView, this enum is obsolete. Please use CollectionView instead.")]
	public enum ListViewSelectionMode
	{
		/// <summary>
		/// Allows ListItems to have TapGestures. The Enter key and Narrator will not fire the ItemTapped event.
		/// </summary>
		Inaccessible,
		/// <summary>
		/// Allows the Enter key and Narrator to fire the ItemTapped event. ListItems cannot have TapGestures.
		/// </summary>
		Accessible
	}
}
