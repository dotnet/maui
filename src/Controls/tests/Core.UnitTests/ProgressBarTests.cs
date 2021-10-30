using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ProgressBarTests : BaseTestFixture
	{
		[Test]
		public void TestClamp()
		{
			ProgressBar bar = new ProgressBar();

			bar.Progress = 2;
			Assert.AreEqual(1, bar.Progress);

			bar.Progress = -1;
			Assert.AreEqual(0, bar.Progress);
		}

		[Test]
		public void TestProgressTo()
		{
			var bar = AnimationReadyHandler.Prepare(new ProgressBar());

			bar.ProgressTo(0.8, 250, Easing.Linear);

			Assert.That(bar.Progress, Is.EqualTo(0.8).Within(0.001));
		}
	}
}