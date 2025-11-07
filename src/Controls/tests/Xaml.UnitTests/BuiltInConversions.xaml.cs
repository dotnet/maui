using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BuiltInConversions : ContentPage
{
	public BuiltInConversions() => InitializeComponent();


	public class Tests : IDisposable
	{

		public Tests()
		{
			Application.Current = new MockApplication();
		}
		public void Dispose()
		{
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void Datetime(XamlInflator inflator)
		{
			var layout = new BuiltInConversions(inflator);

			Assert.Equal(new DateTime(2015, 01, 16), layout.datetime0.Date);
			Assert.Equal(new DateTime(2015, 01, 16), layout.datetime1.Date);
		}

		[Theory]
		[Values]
		public void String(XamlInflator inflator)
		{
			var layout = new BuiltInConversions(inflator);

			Assert.Equal("foobar", layout.label0.Text);
			Assert.Equal("foobar", layout.label1.Text);

			//Issue #2122, implicit content property not trimmed
			Assert.Equal("foobar", layout.label2.Text);
		}
	}
}