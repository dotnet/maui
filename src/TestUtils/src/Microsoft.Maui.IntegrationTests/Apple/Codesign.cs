using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests.Apple
{
	public static class Codesign
	{
		public static List<string> SearchForExpectedEntitlements(
			string entitlementsPath,
			string appLocation,
			List<string> expectedEntitlements)
		{
			List<string> foundEntitlements = new();
			string procOutput = ToolRunner.Run(new ProcessStartInfo()
			{
				FileName = "/usr/bin/codesign",
				Arguments = $"-d --entitlements {entitlementsPath} --xml {appLocation}"
			}, out int errorCode);

			Assert.AreEqual(errorCode, 0, procOutput);
			Assert.IsTrue(File.Exists(entitlementsPath));

			string fileContent = File.ReadAllText(entitlementsPath);
			foreach (string entitlement in expectedEntitlements)
			{
				if (fileContent.Contains(entitlement, StringComparison.OrdinalIgnoreCase))
					foundEntitlements.Add(entitlement);
			}

			return foundEntitlements;
		}
	}
}

