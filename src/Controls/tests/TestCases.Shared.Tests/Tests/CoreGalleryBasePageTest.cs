using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class CoreGalleryBasePageTest : _GalleryUITest
	{
		public CoreGalleryBasePageTest(TestDevice device) : base(device) { }

		protected override void FixtureSetup()
		{
			int retries = 0;
			while (true)
			{
				try
				{
					base.FixtureSetup();
#if MACCATALYST
					NavigateToGallery();
#endif
					break;
				}
				catch (Exception e)
				{
					TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureSetup threw an exception. Attempt {retries}/{SetupMaxRetries}.{Environment.NewLine}Exception details: {e}");
					if (retries++ < SetupMaxRetries)
					{
						App.Back();
						Reset();
					}
					else
					{
						throw;
					}
				}
			}
		}

		protected abstract void NavigateToGallery();
	}
}
