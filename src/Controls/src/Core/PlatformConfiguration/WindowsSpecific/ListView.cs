#nullable disable
using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.ListView;

	/// <summary>Platform-specific properties for list view controls on UWP.</summary>
	public static class ListView
	{
		#region SelectionMode

		/// <summary>Bindable property for <see cref="SelectionMode"/>.</summary>
		public static readonly BindableProperty SelectionModeProperty =
			BindableProperty.CreateAttached("WindowsSelectionMode", typeof(ListViewSelectionMode),
				typeof(ListView), ListViewSelectionMode.Accessible);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='GetSelectionMode'][1]/Docs/*" />
		public static ListViewSelectionMode GetSelectionMode(BindableObject element)
		{
			return (ListViewSelectionMode)element.GetValue(SelectionModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='SetSelectionMode'][1]/Docs/*" />
		public static void SetSelectionMode(BindableObject element, ListViewSelectionMode value)
		{
			element.SetValue(SelectionModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='GetSelectionMode'][2]/Docs/*" />
		public static ListViewSelectionMode GetSelectionMode(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (ListViewSelectionMode)config.Element.GetValue(SelectionModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/ListView.xml" path="//Member[@MemberName='SetSelectionMode'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetSelectionMode(
			this IPlatformElementConfiguration<Windows, FormsElement> config, ListViewSelectionMode value)
		{
			config.Element.SetValue(SelectionModeProperty, value);
			return config;
		}

		#endregion
	}

	/// <summary>Selection modes for list view controls on UWP.</summary>
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
