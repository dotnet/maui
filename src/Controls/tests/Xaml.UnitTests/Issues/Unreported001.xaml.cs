using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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
		public class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(true)]
			public void DoesNotThrow(bool useCompiledXaml)
			{
				var p = new Unreported001(useCompiledXaml);
				Assert.That(p.navpage.CurrentPage, Is.TypeOf<U001Page>());
			}
		}
	}
}