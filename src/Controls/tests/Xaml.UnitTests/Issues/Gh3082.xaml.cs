using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh3082 : ContentPage
	{
		public Gh3082()
		{
			InitializeComponent();
		}

		public Gh3082(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		async Task OnClicked(object sender, EventArgs e)
		{
			await Task.Delay(1000);
		}		class Tests
		{
			[InlineData(false), InlineData(true)]
			public void ThrowsOnWrongEventHandlerSignature(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh3082)));
				else
					Assert.Throws<XamlParseException>(() => new Gh3082(false));
			}
		}
	}
}
