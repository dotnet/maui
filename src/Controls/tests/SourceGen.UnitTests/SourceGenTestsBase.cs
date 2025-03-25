using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class SourceGenTestsBase
{
	public static void VerifyStepRunReasons(GeneratorRunResult result2, Dictionary<string, IncrementalStepRunReason> expectedReasons)
	{
		foreach (var expected in expectedReasons)
		{
			var actualReason = result2.TrackedSteps[expected.Key].Single().Outputs.Single().Reason;
			Assert.AreEqual(expected.Value, actualReason, message: expected.Key);
		}
	}
}