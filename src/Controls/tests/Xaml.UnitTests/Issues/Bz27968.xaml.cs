using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz27968Page : ContentPage
	{
	}

	public partial class Bz27968 : Bz27968Page
	{
		public Bz27968()
		{
			InitializeComponent();
		}

		public Bz27968(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[TestCase(true)]
			[TestCase(false)]
			public void BaseClassIdentifiersAreValidForResources(bool useCompiledXaml)
			{
				var layout = new Bz27968(useCompiledXaml);
				Assert.That(layout.Resources["listView"], Is.TypeOf<ListView>());
			}
		}
	}
}
