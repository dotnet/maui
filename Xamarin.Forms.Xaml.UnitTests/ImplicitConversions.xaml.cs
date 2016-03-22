using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class ImplicitConversions : ContentPage
	{
		public ImplicitConversions ()
		{
			InitializeComponent ();
		}

		[TestFixture]
		public class Tests
		{
			[Test]
			public void Datetime ()
			{
				var layout = new ImplicitConversions ();

				Assert.AreEqual (new DateTime (2015, 01, 16), layout.datetime0.Date);
				Assert.AreEqual (new DateTime (2015, 01, 16), layout.datetime1.Date);
			}

			[Test]
			public void String ()
			{
				var layout = new ImplicitConversions ();

				Assert.AreEqual ("foobar", layout.label0.Text);
				Assert.AreEqual ("foobar", layout.label1.Text);

				//Issue #2122, implicit content property not trimmed
				Assert.AreEqual ("foobar", layout.label2.Text);
			}
		}
	}
}