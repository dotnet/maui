using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh3606 : ContentPage
	{
		public Gh3606()
		{
			InitializeComponent();
		}

		public Gh3606(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}


		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[TestCase(true)]
			public void BindingsWithSourceArentCompiled(bool useCompiledXaml)
			{
				new Gh3606(useCompiledXaml);
			}
		}
	}
}
