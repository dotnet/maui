using System.Diagnostics;

namespace Microsoft.Maui.DeviceTemplateTests.Apple
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

			Assert.Equal(0, errorCode);
			if (errorCode != 0)
				throw new ArgumentException($"Codesign failed with error code {errorCode}: {procOutput}");
			Assert.True(File.Exists(entitlementsPath));

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

