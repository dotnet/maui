using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class GrialIssue01 : ContentPage
	{
		public GrialIssue01()
		{
			InitializeComponent();
		}

		public GrialIssue01(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void ImplicitCastIsUsedOnFileImageSource(bool useCompiledXaml)
			{
				var layout = new GrialIssue01(useCompiledXaml);
				var res = (FileImageSource)layout.Resources["image"];

				Assert.Equal("path.png", res.File);
			}
		}
	}
}