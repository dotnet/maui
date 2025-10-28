using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz60203 : ContentPage
	{
		public Bz60203()
		{
			InitializeComponent();
		}

		public Bz60203(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{

			[InlineData(true), TestCase(false)]
			public void CanCompileMultiTriggersWithDifferentConditions(bool useCompiledXaml)
			{
				var layout = new Bz60203(useCompiledXaml);
				Assert.Equal(BackgroundColorProperty.DefaultValue, layout.label.BackgroundColor);
				layout.BindingContext = new { Text = "Foo" };
				layout.label.TextColor = Colors.Blue;
				Assert.Equal(Colors.Pink, layout.label.BackgroundColor);
			}

		}
	}
}