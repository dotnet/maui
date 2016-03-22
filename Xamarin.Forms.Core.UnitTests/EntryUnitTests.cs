using System.Diagnostics;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class EntryUnitTests : BaseTestFixture
	{
		[Test]
		public void ValueChangedFromSetValue()
		{
			var entry = new Entry();

			const string value = "Foo";

			bool signaled = false;
			entry.TextChanged += (sender, args) => {
				signaled = true;
				Assert.AreEqual (value, args.NewTextValue);
			};

			entry.SetValue (Entry.TextProperty, value);

			Assert.IsTrue (signaled, "ValueChanged did not fire");
		}

		[TestCase (null, "foo")]
		[TestCase ("foo", "bar")]
		[TestCase ("foo", null)]
		public void ValueChangedArgs (string initial, string final)
		{
			var entry = new Entry {
				Text = initial
			};

			string oldValue = null;
			string newValue = null;

			Entry entryFromSender = null;

			entry.TextChanged += (s, e) => {
				entryFromSender = (Entry)s;
				oldValue = e.OldTextValue;
				newValue = e.NewTextValue;
			};

			entry.Text = final;

			Assert.AreEqual (entry, entryFromSender);
			Assert.AreEqual (initial, oldValue);
			Assert.AreEqual (final, newValue);
		}
	}
}
