using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh5706 : Shell
	{
		public Gh5706() => InitializeComponent();
		public Gh5706(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Fact]
			public void ReportSyntaxError([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh5706(useCompiledXaml);
				layout.searchHandler.BindingContext = new Gh5706VM();

				Assert.Null(layout.searchHandler.CommandParameter);
				layout.searchHandler.Query = "Foo";
				Assert.Equal("Foo", layout.searchHandler.CommandParameter);
			}
		}
	}

	class Gh5706VM
	{
		public Gh5706VM()
		{
			FilterCommand = new Command((p) => Param = p);
		}

		public Command FilterCommand { get; set; }

		public object Param { get; set; }
	}
}
