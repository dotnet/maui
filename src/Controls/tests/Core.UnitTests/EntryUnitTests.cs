using System.Diagnostics;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class EntryUnitTests : BaseTestFixture
	{
		[Fact]
		public void ValueChangedFromSetValue()
		{
			var entry = new Entry();

			const string value = "Foo";

			bool signaled = false;
			entry.TextChanged += (sender, args) =>
			{
				signaled = true;
				Assert.Equal(value, args.NewTextValue);
			};

			entry.SetValue(Entry.TextProperty, value);

			Assert.True(signaled, "ValueChanged did not fire");
		}

		[Theory]
		[InlineData(null, "foo")]
		[InlineData("foo", "bar")]
		[InlineData("foo", null)]
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

			Assert.Equal(entry, entryFromSender);
			Assert.Equal(initial, oldValue);
			Assert.Equal(final, newValue);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(0)]
		[InlineData(9999)]
		public void CursorPositionValid(int val)
		{
			var entry = new Entry
			{
				CursorPosition = val
			};

			var target = entry.CursorPosition;

			Assert.Equal(target, val);
		}

		[Fact]
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

		[Theory]
		[InlineData(1)]
		[InlineData(0)]
		[InlineData(9999)]
		public void SelectionLengthValid(int val)
		{
			var entry = new Entry
			{
				SelectionLength = val
			};

			var target = entry.SelectionLength;

			Assert.Equal(target, val);
		}

		[Fact]
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void IsReadOnlyTest(bool isReadOnly)
		{
			Entry entry = new Entry();
			entry.SetValue(InputView.IsReadOnlyProperty, isReadOnly);
			Assert.Equal(isReadOnly, entry.IsReadOnly);
		}

		[Fact]
		public void IsReadOnlyDefaultValueTest()
		{
			Entry entry = new Entry();
			Assert.False(entry.IsReadOnly);
		}
	}
}
