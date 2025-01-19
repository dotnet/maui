using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24414 : _IssuesUITest
	{
		public Issue24414(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadows not rendering as expected on Android and iOS";

		[Test]
		[Category(UITestCategories.Visual)]
		public void Issue24414Test()
		{
			// TODO: Refactor after collecting new screenshot from AZP
			// - TryVerifyScreenshot => VerifyScreenshot
			// - Remove duplicated call to `VerifyMemory`

			var label = App.WaitForElement("TheLabel");
			var ex = TryVerifyScreenshot("Issue24414Test");
#if ANDROID
			var memorySnapshot = App.GetPerformanceMemoryInfo();
#endif

			for (int i = 1; i <= 5; i++)
			{
				label.Tap();
				var exception = TryVerifyScreenshot("Issue24414Test_" + i);
				ex ??= exception;
			}

			if (ex != null)
			{
#if ANDROID
				VerifyMemory(memorySnapshot, App.GetPerformanceMemoryInfo());
#endif

				throw ex;
			}

#if ANDROID
			VerifyMemory(memorySnapshot, App.GetPerformanceMemoryInfo());
#endif
		}

#if ANDROID
		static void VerifyMemory(IReadOnlyDictionary<string, int> memoryBefore, IReadOnlyDictionary<string, int> memoryAfter)
		{
			var allowedMemoryIncrease = new Dictionary<string, double>
			{
				{ "totalPrivateDirty", 0.01 },
				{ "nativePrivateDirty", 0 },
				{ "dalvikPrivateDirty", 0.19 },
				{ "totalPss", 0.01 },
				{ "nativePss", 0 },
				{ "dalvikPss", 0.18 },
				{ "nativeHeapAllocatedSize", 0.001 },
				{ "nativeHeapSize", 0.015 },
				{ "totalRss", 0.01 },
				{ "nativeRss", 0 },
				{ "dalvikRss", 0.1 }
			};

			foreach (KeyValuePair<string,int> counter in memoryAfter)
			{
				var delta = (counter.Value - memoryBefore[counter.Key]) / (double)memoryBefore[counter.Key];
				var allowedIncrease = allowedMemoryIncrease[counter.Key];
				ClassicAssert.LessOrEqual(delta, allowedIncrease, $"Memory counter {counter.Key} increased by {delta * 100}% which is more than the expected {allowedIncrease * 100}%");
			}
		}
#endif

		private Exception? TryVerifyScreenshot(string fileName)
		{
			try
			{
				VerifyScreenshot(fileName);
			}
			catch(Exception ex)
			{
				return ex;
			}

			return null;
		}
	}
}
