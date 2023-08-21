// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NUnit.Framework;
using VisualTestUtils;

namespace Microsoft.Maui.AppiumTests
{
	public class VisualTestContext : ITestContext
	{
		public void AddTestAttachment(string filePath, string? description = null) =>
			TestContext.AddTestAttachment(filePath, description);
	}
}
