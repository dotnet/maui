using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class SimpleContentPageCode : ContentPage
	{
		public SimpleContentPageCode()
		{
			Content = new Label
			{
				Text = "Hello, Microsoft.Maui.Controls!",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
		}

		public SimpleContentPageCode(bool useCompiledXaml) : this()
		{
		}
	}
	public partial class SimpleContentPage : ContentPage
	{
		public SimpleContentPage()
		{
			InitializeComponent();
		}

		public SimpleContentPage(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public public class Tests
		{
			[Fact(Skip = nameof(XamlCIs20TimesFasterThanXaml))]
			public void XamlCIs20TimesFasterThanXaml()
			{
				var swXamlC = new Stopwatch();
				var swXaml = new Stopwatch();

				swXamlC.Start();
				for (var i = 0; i < 1000; i++)
					new SimpleContentPage(true);
				swXamlC.Stop();

				swXaml.Start();
				for (var i = 0; i < 1000; i++)
					new SimpleContentPage(false);
				swXaml.Stop();

				Assert.True(swXamlC.ElapsedMilliseconds * 20 < swXaml.ElapsedMilliseconds,
					$"Expected XamlC to be at least 20x faster than Xaml, but {swXamlC.ElapsedMilliseconds * 20} >= {swXaml.ElapsedMilliseconds}");
			}

			[Fact(Skip = nameof(XamlCIsNotMuchSlowerThanCode))]
			public void XamlCIsNotMuchSlowerThanCode()
			{
				var swXamlC = new Stopwatch();
				var swCode = new Stopwatch();

				swXamlC.Start();
				for (var i = 0; i < 1000; i++)
					new SimpleContentPage(true);
				swXamlC.Stop();

				swCode.Start();
				for (var i = 0; i < 1000; i++)
					new SimpleContentPageCode(false);
				swCode.Stop();

				Assert.True(swXamlC.ElapsedMilliseconds * .2 <= swCode.ElapsedMilliseconds,
					$"Expected XamlC to be not much slower than code, but {swXamlC.ElapsedMilliseconds * .2} > {swCode.ElapsedMilliseconds}");
			}
		}
	}
}