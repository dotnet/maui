using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue31280 : ContentPage
{
	public Issue31280()
	{
		InitializeComponent();
	}

	public Issue31280(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	public class Tests
	{
		[TestCase(false)]
		[TestCase(true)]
		public void StyleInheritanceShouldWork(bool useCompiledXaml)
		{
			var layout = new Issue31280(useCompiledXaml);
			Assert.AreEqual(Colors.Green, layout.LabelBaseStyle.TextColor);
			Assert.AreEqual(Colors.Red, layout.LabelBaseStyleRed.TextColor);
			Assert.AreEqual(Colors.Red, layout.LabelWithPadding.TextColor);
		}
	}
}