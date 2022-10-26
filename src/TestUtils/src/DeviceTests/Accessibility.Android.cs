using System.Collections.Generic;
using System.Text;
using AccessibilityCheck.Droid;
using AccessibilityCheck.Droid.Checks;
using AccessibilityCheck.Droid.UIElement;
using Xunit.Sdk;

namespace Microsoft.Maui.TestUtils.DeviceTests
{
	public class Accessibility
	{
		public static void AssertAccessible(Android.Views.View root)
		{
			List<AccessibilityHierarchyCheck> checks = new List<AccessibilityHierarchyCheck>()
					{
						new ClassNameCheck(),
						new DuplicateClickableBoundsCheck(),
						new DuplicateSpeakableTextCheck(),
						new EditableContentDescCheck(),
						new RedundantDescriptionCheck(),
						new SpeakableTextPresentCheck(),
						new TouchTargetSizeCheck(),
					};

			var hierarchy = AccessibilityHierarchyAndroid.NewBuilder(root).Build();

			var results = RunChecks(hierarchy, checks);

			AssertChecksPassed(results);
		}

		static List<AccessibilityHierarchyCheckResult> RunChecks(AccessibilityHierarchy hierarchy, 
			List<AccessibilityHierarchyCheck> checks) 
		{
			List<AccessibilityHierarchyCheckResult> results = new();

			for(int c = 0; c < checks.Count; c++)
			{
				var checkResult = checks[c].RunCheckOnHierarchy(hierarchy);

				for(int r = 0; r < checkResult.Count; r++)
				{
					results.Add(checkResult[r]);
				}
			}

			return results;
		}

		static void AssertChecksPassed(List<AccessibilityHierarchyCheckResult> results) 
		{
			var errors = new List<AccessibilityHierarchyCheckResult>();

			for (int n = 0; n < results.Count; n++)
			{
				var result = results[n];

				if (result.Type == AccessibilityCheckResult.AccessibilityCheckResultType.Error)
				{
					errors.Add(result);
				}
			}

			if (errors.Count > 0)
			{
				throw new XunitException(BuildErrorMessage(errors));
			}
		}

		static string BuildErrorMessage(List<AccessibilityHierarchyCheckResult> errors) 
		{
			StringBuilder sb = new StringBuilder();

			for(int n = 0; n < errors.Count; n++)
			{
				sb.Append(errors[n].Message);
			}

			return sb.ToString();
		}
	}
}
