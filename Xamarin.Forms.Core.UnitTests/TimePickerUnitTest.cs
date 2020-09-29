using System;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TimePickerUnitTest : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			TimePicker picker = new TimePicker();

			Assert.AreEqual(new TimeSpan(), picker.Time);
		}

		[Test]
		public void TestTimeOutOfRange()
		{
			TimePicker picker = new TimePicker();

			Assert.That(() => picker.Time = new TimeSpan(1000, 0, 0), Throws.ArgumentException);
			Assert.AreEqual(picker.Time, new TimeSpan());

			picker.Time = new TimeSpan(8, 30, 0);

			Assert.AreEqual(new TimeSpan(8, 30, 0), picker.Time);

			Assert.That(() => picker.Time = new TimeSpan(-1, 0, 0), Throws.ArgumentException);
			Assert.AreEqual(new TimeSpan(8, 30, 0), picker.Time);
		}

		[Test]
		[Description("Issue #745")]
		public void ZeroTimeIsValid()
		{
			var picker = new TimePicker();

			Assert.That(() => picker.Time = new TimeSpan(0, 0, 0), Throws.Nothing);
		}
	}
}