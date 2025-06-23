using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh5706 : Shell
	{
		public Gh5706() => InitializeComponent();
		public Gh5706(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Test]
			public void ReportSyntaxError([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh5706(useCompiledXaml);
				layout.searchHandler.BindingContext = new Gh5706VM();

				Assert.That(layout.searchHandler.CommandParameter, Is.Null);
				layout.searchHandler.Query = "Foo";
				Assert.That(layout.searchHandler.CommandParameter, Is.EqualTo("Foo"));
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
