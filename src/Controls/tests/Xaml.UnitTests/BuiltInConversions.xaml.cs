using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class BuiltInConversions : ContentPage
	{
		public BuiltInConversions()
		{
			InitializeComponent();
		}

		public BuiltInConversions(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void Datetime(bool useCompiledXaml)
			{
				var layout = new BuiltInConversions(useCompiledXaml);

				Assert.Equal(new DateTime(2015, 01, 16), layout.datetime0.Date);
				Assert.Equal(new DateTime(2015, 01, 16), layout.datetime1.Date);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void String(bool useCompiledXaml)
			{
				var layout = new BuiltInConversions(useCompiledXaml);

				Assert.Equal("foobar", layout.label0.Text);
				Assert.Equal("foobar", layout.label1.Text);

				//Issue #2122, implicit content property not trimmed
				Assert.Equal("foobar", layout.label2.Text);
			}
		}
	}
}