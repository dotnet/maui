using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh6176VM
	{
	}

	public class Gh6176Base<TVM> : ContentPage where TVM: class
	{
		public TVM ViewModel => BindingContext as TVM;
		protected void ShowMenu(object sender, EventArgs e) { }
	}

	public partial class Gh6176
	{
		public Gh6176() => InitializeComponent();
		public Gh6176(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture] class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void XamlCDoesntFail([Values(false, true)]bool useCompiledXaml)
			{
				var layout = new Gh6176(useCompiledXaml);
			}
		}
	}
}
