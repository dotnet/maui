using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TapGestureRecognizerTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var tap = new TapGestureRecognizer();

			Assert.AreEqual(null, tap.Command);
			Assert.AreEqual(null, tap.CommandParameter);
			Assert.AreEqual(1, tap.NumberOfTapsRequired);
		}

		[Test]
		public void CallbackPassesParameter()
		{
			var view = new View();
			var tap = new TapGestureRecognizer();
			tap.CommandParameter = "Hello";

			object result = null;
			tap.Command = new Command(o => result = o);

			tap.SendTapped(view);
			Assert.AreEqual(result, tap.CommandParameter);
		}
	}
}