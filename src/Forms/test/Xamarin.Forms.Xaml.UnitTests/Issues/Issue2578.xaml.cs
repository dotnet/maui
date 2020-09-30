using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue2578 : ContentPage
	{
		public Issue2578()
		{
			InitializeComponent();
		}

		public Issue2578(bool useCompiledXaml)
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
			public void MultipleTriggers(bool useCompiledXaml)
			{
				Issue2578 layout = null;
				Assert.DoesNotThrow(() => layout = new Issue2578(useCompiledXaml));

				Assert.AreEqual(null, layout.label.Text);
				Assert.AreEqual(Color.Default, layout.label.BackgroundColor);
				Assert.AreEqual(Color.Olive, layout.label.TextColor);
				layout.label.Text = "Foo";
				Assert.AreEqual(Color.Red, layout.label.BackgroundColor);
			}
		}
	}
}