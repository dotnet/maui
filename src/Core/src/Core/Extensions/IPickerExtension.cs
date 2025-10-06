using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui
{
	public static class IPickerExtension
	{
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
