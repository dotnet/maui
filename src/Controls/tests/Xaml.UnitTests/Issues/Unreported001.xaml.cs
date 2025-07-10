using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class U001Page : ContentPage
	{
		public U001Page()
		{
			;
		}

	}

	public partial class Unreported001 : TabbedPage
	{
		public Unreported001()
		{
			InitializeComponent();
		}

		public Unreported001(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[TestCase(false)]
			[TestCase(true)]
			public void DoesNotThrow(bool useCompiledXaml)
			{
				var p = new Unreported001(useCompiledXaml);
				Assert.That(p.navpage.CurrentPage, Is.TypeOf<U001Page>());
			}
		}
	}
}