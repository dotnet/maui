using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue1438 : ContentPage
	{
		public Issue1438()
		{
			InitializeComponent();
		}

		public Issue1438(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TestCase(false)]
			[TestCase(true)]
			public void XNameForwardDeclaration(bool useCompiledXaml)
			{
				var page = new Issue1438(useCompiledXaml);

				var slider = page.FindByName<Slider>("slider");
				var label = page.FindByName<Label>("label");
				Assert.AreSame(slider, label.BindingContext);
				Assert.That(slider.Parent, Is.TypeOf<StackLayout>());
			}
		}
	}
}