using System;

namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.ListView;

	public static class ListView
	{
		#region SelectionMode

		public static readonly BindableProperty SelectionModeProperty =
			BindableProperty.CreateAttached("SelectionMode", typeof(ListViewSelectionMode),
				typeof(ListView), ListViewSelectionMode.Accessible);

		public static ListViewSelectionMode GetSelectionMode(BindableObject element)
		{
			return (ListViewSelectionMode)element.GetValue(SelectionModeProperty);
		}

		public static void SetSelectionMode(BindableObject element, ListViewSelectionMode value)
		{
			element.SetValue(SelectionModeProperty, value);
		}

		public static ListViewSelectionMode GetSelectionMode(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (ListViewSelectionMode)config.Element.GetValue(SelectionModeProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetSelectionMode(
			this IPlatformElementConfiguration<Windows, FormsElement> config, ListViewSelectionMode value)
		{
			config.Element.SetValue(SelectionModeProperty, value);
			return config;
		}

		#endregion
	}

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