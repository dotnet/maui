using Microsoft.Maui.Animations;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ProgressBarTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();

			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();

			Device.PlatformServices = null;
		}

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
			var bar = new ProgressBar
			{
				Handler = new HandlerWithAnimationContextStub()
			};

			bar.ProgressTo(0.8, 250, Easing.Linear);

			Assert.That(bar.Progress, Is.EqualTo(0.8).Within(0.001));
		}
	}
}
