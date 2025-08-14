using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BuiltInConversions : ContentPage
{
	public BuiltInConversions() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => Application.Current = new MockApplication();
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void Datetime([Values] XamlInflator inflator)
		{
			var layout = new BuiltInConversions(inflator);

			Assert.AreEqual(new DateTime(2015, 01, 16), layout.datetime0.Date);
			Assert.AreEqual(new DateTime(2015, 01, 16), layout.datetime1.Date);
		}

		[Test]
		public void String([Values] XamlInflator inflator)
		{
			var layout = new BuiltInConversions(inflator);

			Assert.AreEqual("foobar", layout.label0.Text);
			Assert.AreEqual("foobar", layout.label1.Text);

			//Issue #2122, implicit content property not trimmed
			Assert.AreEqual("foobar", layout.label2.Text);
		}
	}
}