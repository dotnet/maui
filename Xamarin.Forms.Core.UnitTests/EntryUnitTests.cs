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
			entry.TextChanged += (sender, args) =>
			{
				signaled = true;
				Assert.AreEqual(value, args.NewTextValue);
			};

			entry.SetValue(Entry.TextProperty, value);

			Assert.IsTrue(signaled, "ValueChanged did not fire");
		}

		[TestCase(null, "foo")]
		[TestCase("foo", "bar")]
		[TestCase("foo", null)]
		public void ValueChangedArgs(string initial, string final)
		{
			var entry = new Entry
			{
				Text = initial
			};

			string oldValue = null;
			string newValue = null;

			Entry entryFromSender = null;

			entry.TextChanged += (s, e) =>
			{
				entryFromSender = (Entry)s;
				oldValue = e.OldTextValue;
				newValue = e.NewTextValue;
			};

			entry.Text = final;

			Assert.AreEqual(entry, entryFromSender);
			Assert.AreEqual(initial, oldValue);
			Assert.AreEqual(final, newValue);
		}


		[TestCase(1)]
		[TestCase(0)]
		[TestCase(9999)]
		public void CursorPositionValid(int val)
		{
			var entry = new Entry
			{
				CursorPosition = val
			};

			var target = entry.CursorPosition;

			Assert.AreEqual(target, val);
		}

		[Test]
		public void CursorPositionInvalid()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				var entry = new Entry
				{
					CursorPosition = -1
				};
			});
		}

		[TestCase(1)]
		[TestCase(0)]
		[TestCase(9999)]
		public void SelectionLengthValid(int val)
		{
			var entry = new Entry
			{
				SelectionLength = val
			};

			var target = entry.SelectionLength;

			Assert.AreEqual(target, val);
		}

		[Test]
		public void SelectionLengthInvalid()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				var entry = new Entry
				{
					SelectionLength = -1
				};
			});
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReturnTypeCommand(bool isEnabled)
		{
			var entry = new Entry()
			{
				IsEnabled = isEnabled,
			};

			bool result = false;

			var bindingContext = new
			{
				Command = new Command(() => { result = true; }, () => true)
			};

			entry.SetBinding(Entry.ReturnCommandProperty, "Command");
			entry.BindingContext = bindingContext;

			entry.SendCompleted();

			Assert.True(result == isEnabled ? true : false);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReturnTypeCommandNullTestIsEnabled(bool isEnabled)
		{
			var entry = new Entry()
			{
				IsEnabled = isEnabled,
			};

			bool result = false;

			entry.SetBinding(Entry.ReturnCommandProperty, "Command");
			entry.BindingContext = null;
			entry.Completed += (s, e) =>
			{
				result = true;
			};
			entry.SendCompleted();

			Assert.True(result == isEnabled ? true : false);
		}

		[TestCase(true)]
		public void IsReadOnlyTest(bool isReadOnly)
		{
			Entry entry = new Entry();
			entry.SetValue(InputView.IsReadOnlyProperty, isReadOnly);
			Assert.AreEqual(isReadOnly, entry.IsReadOnly);
		}

		[Test]
		public void IsReadOnlyDefaultValueTest()
		{
			Entry entry = new Entry();
			Assert.AreEqual(entry.IsReadOnly, false);
		}
	}
}