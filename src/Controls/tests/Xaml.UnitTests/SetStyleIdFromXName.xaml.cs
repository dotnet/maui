using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class SetStyleIdFromXName : ContentPage
	{
		public SetStyleIdFromXName() => InitializeComponent();
		public SetStyleIdFromXName(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false), InlineData(true)]
			public void SetStyleId(bool useCompiledXaml)
			{
				var layout = new SetStyleIdFromXName(useCompiledXaml);
				Assert.Equal("label0", layout.label0.StyleId);
				Assert.Equal("foo", layout.label1.StyleId);
				Assert.Equal("bar", layout.label2.StyleId);
			}
		}
	}
}
