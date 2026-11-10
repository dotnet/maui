using System.Collections.Generic;

namespace Microsoft.Maui
{
	public static class IPickerExtension
	{
		internal static bool ShouldShowClearButton(this IPicker picker) =>
			picker is IPickerClearButton { IsClearButtonVisible: true } && picker.SelectedIndex >= 0;

		internal static bool ClearSelection(this IPicker picker)
		{
			if (!picker.IsEnabled || !picker.ShouldShowClearButton())
			{
				return false;
			}

			picker.SelectedIndex = -1;
			return true;
		}

		public static string[] GetItemsAsArray(this IPicker picker)
		{
			var returnValue = new string[picker.GetCount()];
			for (int i = 0; i < returnValue.Length; i++)
				returnValue[i] = picker.GetItem(i);

			return returnValue;
		}

		public static List<string> GetItemsAsList(this IPicker picker)
		{
			var numberOfElements = picker.GetCount();
			var returnValue = new List<string>(numberOfElements);
			for (int i = 0; i < numberOfElements; i++)
				returnValue.Add(picker.GetItem(i));

			return returnValue;
		}
	}
}
