using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class ImplicitResourceDictionaries : ContentPage
	{
		public ImplicitResourceDictionaries()
		{
			InitializeComponent();
		}

		public ImplicitResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ImplicitRDonContentViews(bool useCompiledXaml)
			{
				var layout = new ImplicitResourceDictionaries(useCompiledXaml);
				Assert.Equal(Colors.Purple, layout.label.TextColor);
			}
		}
	}
}
