using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Microsoft.Maui.DeviceTests
{
	public static class DeviceTestSharedHelpers
	{
		public static string[] GetTestCategoryValues([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] this Type testCategoryType)
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

		public static List<String> GetExcludedTestCategories([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] this Type testCategoryType)
		{
#if IOS || MACCATALYST
			foreach (var en in Foundation.NSProcessInfo.ProcessInfo.Environment)
			{
				string key = $"{en.Key}";
				string filterValue = $"{en.Value}";

				// Support TestFilter=Category=X (run only category X)
				if (key == "TestFilter" && filterValue.StartsWith("Category="))
				{
					Console.WriteLine($"TestFilter: {filterValue}");
					string categoryToRun = $"{filterValue.Split('=')[1]}";
					var categories = new List<String>(GetTestCategoryValues(testCategoryType));
					categories.Remove(categoryToRun);
					return categories.Select(c => $"Category={c}").ToList();
				}

				// Support SkipCategories=X,Y,Z (skip categories X, Y, Z)
				if (key == "SkipCategories")
				{
					Console.WriteLine($"SkipCategories: {filterValue}");
					var categoriesToSkip = filterValue.Split(',').Select(c => c.Trim()).ToList();
					return categoriesToSkip.Select(c => $"Category={c}").ToList();
				}
			}
#endif
			return new List<String>();
		}
	}
}