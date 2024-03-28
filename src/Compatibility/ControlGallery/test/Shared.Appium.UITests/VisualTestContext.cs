using NUnit.Framework;
using VisualTestUtils;

namespace UITests
{
	public class VisualTestContext : ITestContext
	{
		public void AddTestAttachment(string filePath, string? description = null) =>
			TestContext.AddTestAttachment(filePath, description);
	}
}