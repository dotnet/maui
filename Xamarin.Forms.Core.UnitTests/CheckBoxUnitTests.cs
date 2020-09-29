using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class CheckBoxUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			var checkBox = new CheckBox();

			Assert.IsFalse(checkBox.IsChecked);
		}

		[Test]
		public void TestOnEvent()
		{
			var checkBox = new CheckBox();

			var fired = false;
			checkBox.CheckedChanged += (sender, e) => fired = true;

			checkBox.IsChecked = true;

			Assert.IsTrue(fired);
		}

		[Test]
		public void TestOnEventNotDoubleFired()
		{
			var checkBox = new CheckBox();

			bool fired = false;
			checkBox.IsChecked = true;

			checkBox.CheckedChanged += (sender, args) => fired = true;
			checkBox.IsChecked = true;

			Assert.IsFalse(fired);
		}
	}

}