using System;
using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class CommandTests : BaseTestFixture
	{
		[Test]
		public void Constructor ()
		{
			var cmd = new Command (() => { });
			Assert.True (cmd.CanExecute (null));
		}

		[Test]
		public void ThrowsWithNullConstructor ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command ((Action)null));
		}

		[Test]
		public void ThrowsWithNullParameterizedConstructor ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command ((Action<object>)null));
		}

		[Test]
		public void ThrowsWithNullCanExecute ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command (() => { }, null));
		}

		[Test]
		public void ThrowsWithNullParameterizedCanExecute ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command (o => { }, null));
		}

		[Test]
		public void ThrowsWithNullExecuteValidCanExecute ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command (null, () => true));
		}

		[Test]
		public void Execute ()
		{
			bool executed = false;
			var cmd = new Command (() => executed = true);

			cmd.Execute (null);
			Assert.True (executed);
		}

		[Test]
		public void ExecuteParameterized ()
		{
			object executed = null;
			var cmd = new Command (o => executed = o);

			var expected = new object ();
			cmd.Execute (expected);

			Assert.AreEqual (expected, executed);
		}

		[Test]
		public void ExecuteWithCanExecute ()
		{
			bool executed = false;
			var cmd = new Command (() => executed = true, () => true);

			cmd.Execute (null);
			Assert.True (executed);
		}

		[Test]
		public void CanExecute ([Values (true, false)] bool expected)
		{
			bool canExecuteRan = false;
			var cmd = new Command (() => { }, () => {
				canExecuteRan = true;
				return expected;
			});

			Assert.AreEqual(expected, cmd.CanExecute (null));
			Assert.True (canExecuteRan);
		}

		[Test]
		public void ChangeCanExecute ()
		{
			bool signaled = false;
			var cmd = new Command (() => { });

			cmd.CanExecuteChanged += (sender, args) => signaled = true;

			cmd.ChangeCanExecute ();
			Assert.True (signaled);
		}

		[Test]
		public void GenericThrowsWithNullExecute ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command<string> (null));
		}

		[Test]
		public void GenericThrowsWithNullExecuteAndCanExecuteValid ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command<string> (null, s => true));
		}

		[Test]
		public void GenericThrowsWithValidExecuteAndCanExecuteNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command<string> (s => { }, null));
		}

		[Test]
		public void GenericExecute ()
		{
			string result = null;
			var cmd = new Command<string> (s => result = s);

			cmd.Execute ("Foo");
			Assert.AreEqual ("Foo", result);
		}

		[Test]
		public void GenericExecuteWithCanExecute ()
		{
			string result = null;
			var cmd = new Command<string> (s => result = s, s => true);

			cmd.Execute ("Foo");
			Assert.AreEqual ("Foo", result);
		}

		[Test]
		public void GenericCanExecute ([Values (true, false)] bool expected)
		{
			string result = null;
			var cmd = new Command<string> (s => { }, s => {
				result = s;
				return expected;
			});

			Assert.AreEqual (expected, cmd.CanExecute ("Foo"));
			Assert.AreEqual ("Foo", result);
		}
	}
}
