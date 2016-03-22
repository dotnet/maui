using System;

using NUnit.Framework;
using System.Collections.Generic;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class PickerTests : BaseTestFixture
	{
		[Test]
		public void TestSetSelectedIndexOnNullRows()
		{
			var picker = new Picker ();

			Assert.IsEmpty (picker.Items);
			Assert.AreEqual (-1, picker.SelectedIndex);

			picker.SelectedIndex = 2;

			Assert.AreEqual (-1, picker.SelectedIndex);		
		}

		[Test]
		public void TestSelectedIndexInRange ()
		{
			var picker = new Picker { Items =  { "John", "Paul", "George", "Ringo" } };

			picker.SelectedIndex = 2;
			Assert.AreEqual (2, picker.SelectedIndex);

			picker.SelectedIndex = 42;
			Assert.AreEqual (3, picker.SelectedIndex);

			picker.SelectedIndex = -1;
			Assert.AreEqual (-1, picker.SelectedIndex);

			picker.SelectedIndex = -42;
			Assert.AreEqual (-1, picker.SelectedIndex);
		}

		[Test]
		public void TestSelectedIndexChangedOnCollectionShrink()
		{
			var picker = new Picker { Items = { "John", "Paul", "George", "Ringo" }, SelectedIndex = 3 };

			Assert.AreEqual (3, picker.SelectedIndex);

			picker.Items.RemoveAt (3);
			picker.Items.RemoveAt (2);


			Assert.AreEqual (1, picker.SelectedIndex);

			picker.Items.Clear ();
			Assert.AreEqual (-1, picker.SelectedIndex);
		}
	}	
}
