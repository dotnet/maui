#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
#if ANDROID
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
#endif

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
					values.Add((string)field.GetValue(null)!);
				}
			}

			return values.ToArray();
		}

		public static List<String> GetExcludedTestCategories([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] this Type testCategoryType)
		{
			string? filterValue = null;

#if IOS || MACCATALYST
			foreach (var en in Foundation.NSProcessInfo.ProcessInfo.Environment)
			{
				string key = $"{en.Key}";
				if (key == "TestFilter")
				{
					filterValue = $"{en.Value}";
					break;
				}
			}
#elif ANDROID
			// Read TestFilter from instrumentation arguments
			var instrumentation = MauiTestInstrumentation.Current;
			Console.WriteLine($"[GetExcludedTestCategories] MauiTestInstrumentation.Current is null: {instrumentation == null}");
			if (instrumentation != null)
			{
				Console.WriteLine($"[GetExcludedTestCategories] Arguments is null: {instrumentation.Arguments == null}");
				filterValue = instrumentation.Arguments?.GetString("TestFilter");
				Console.WriteLine($"[GetExcludedTestCategories] TestFilter from args: '{filterValue ?? "(null)"}'");
			}
#endif

			if (!string.IsNullOrEmpty(filterValue))
			{
				// Support TestFilter=Category=X (run only category X)
				if (filterValue.StartsWith("Category="))
				{
					Console.WriteLine($"TestFilter: {filterValue}");
					string categoryToRun = $"{filterValue.Split('=')[1]}";
					var categories = new List<String>(GetTestCategoryValues(testCategoryType));
					categories.Remove(categoryToRun);
					return categories.Select(c => $"Category={c}").ToList();
				}

				// Support TestFilter=SkipCategories=X,Y,Z (skip categories X, Y, Z)
				if (filterValue.StartsWith("SkipCategories="))
				{
					Console.WriteLine($"TestFilter: {filterValue}");
					var categoriesToSkip = filterValue.Substring("SkipCategories=".Length)
						.Split(new[] { ',', ';' })
						.Select(c => c.Trim())
						.Where(c => !string.IsNullOrWhiteSpace(c))
						.ToList();
					return categoriesToSkip.Select(c => $"Category={c}").ToList();
				}
			}

			return new List<String>();
		}
	}
}