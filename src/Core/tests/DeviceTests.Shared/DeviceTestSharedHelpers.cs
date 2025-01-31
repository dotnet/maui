using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.DeviceTests
{
	public static class DeviceTestSharedHelpers
	{
		public static string[] GetTestCategoryValues(this Type testCategoryType)
		{
			var values = new List<string>();

			foreach (var field in testCategoryType.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.FieldType == typeof(string))
				{
					values.Add((string)field.GetValue(null));
				}
			}

			return values.ToArray();
		}

		public static List<String> GetExcludedTestCategories(this Type testCategoryType)
		{

#if IOS || MACCATALYST
			foreach (var en in Foundation.NSProcessInfo.ProcessInfo.Environment)
			{
				if ($"{en.Key}" == "TestCategory")
				{
					string categoryToRun = $"{en.Value}";
					var categories = new List<String>(GetTestCategoryValues(testCategoryType));
					categories.Remove(categoryToRun);
					return categories;
				}
			}
#endif
			return new List<String>();
		}
	}
}