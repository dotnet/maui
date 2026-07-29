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
			filterValue = MauiTestInstrumentation.Current?.Arguments?.GetString("TestFilter");
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

		/// <summary>
		/// Returns the list of fully-qualified test class names that should be run exclusively,
		/// read from the <c>IncludeClasses</c> host argument (Android instrumentation arg /
		/// iOS+MacCatalyst environment variable). Value is a comma/semicolon-separated list of
		/// fully-qualified class names (e.g. <c>Microsoft.Maui.DeviceTests.ShellTests</c>).
		/// Returns an empty list when the argument is absent, so normal runs are unaffected.
		/// This is used by the Copilot review gate to verify only a PR's specific test class
		/// instead of its whole <c>Category</c> (which can be blocked by unrelated crashes).
		/// </summary>
		public static List<String> GetIncludedTestClasses(this Type testCategoryType)
		{
			string? includeValue = null;

#if IOS || MACCATALYST
			foreach (var en in Foundation.NSProcessInfo.ProcessInfo.Environment)
			{
				string key = $"{en.Key}";
				if (key == "IncludeClasses")
				{
					includeValue = $"{en.Value}";
					break;
				}
			}
#elif ANDROID
			includeValue = MauiTestInstrumentation.Current?.Arguments?.GetString("IncludeClasses");
#endif

			if (string.IsNullOrWhiteSpace(includeValue))
				return new List<String>();

			Console.WriteLine($"IncludeClasses: {includeValue}");
			return includeValue
				.Split(new[] { ',', ';' })
				.Select(c => c.Trim())
				.Where(c => !string.IsNullOrWhiteSpace(c))
				.ToList();
		}
	}
}