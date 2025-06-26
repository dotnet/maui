using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class AB946693 : ContentPage
	{
		public AB946693() => InitializeComponent();
		public AB946693(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			public void KeylessResourceThrowsMeaningfulException([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(AB946693)));
				else
					Assert.Throws<XamlParseException>(() => new AB946693(useCompiledXaml));
			}
		}
	}
}
