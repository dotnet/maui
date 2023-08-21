// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
			var returnValue = new List<string>(picker.GetCount());
			for (int i = 0; i < returnValue.Count; i++)
				returnValue[i] = picker.GetItem(i);

			return returnValue;
		}
	}
}
