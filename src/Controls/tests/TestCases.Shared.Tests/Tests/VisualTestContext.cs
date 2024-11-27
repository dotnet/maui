﻿using NUnit.Framework;
using VisualTestUtils;

namespace Microsoft.Maui.TestCases.Tests
{
	public class VisualTestContext : ITestContext
	{
		public void AddTestAttachment(string filePath, string? description = null) =>
			TestContext.AddTestAttachment(filePath, description);
	}
}
