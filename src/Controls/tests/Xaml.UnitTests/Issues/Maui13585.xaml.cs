using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{


	public partial class Maui13585 : ContentPage
	{
		public Maui13585() => InitializeComponent();
		public Maui13585(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}



		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void TriggerWithDynamicResource([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui13585(useCompiledXaml);
				Assert.That(page.styleTriggerWithStaticResources.BackgroundColor, Is.EqualTo(Colors.Green));
				Assert.That(page.styleTriggerWithDynamicResources.BackgroundColor, Is.EqualTo(Colors.Green));

				page.styleTriggerWithStaticResources.IsEnabled = false;
				page.styleTriggerWithDynamicResources.IsEnabled = false;

				Assert.That(page.styleTriggerWithStaticResources.BackgroundColor, Is.EqualTo(Colors.Purple));
				Assert.That(page.styleTriggerWithDynamicResources.BackgroundColor, Is.EqualTo(Colors.Purple));

				page.styleTriggerWithStaticResources.IsEnabled = true;
				page.styleTriggerWithDynamicResources.IsEnabled = true;

				Assert.That(page.styleTriggerWithStaticResources.BackgroundColor, Is.EqualTo(Colors.Green));
				Assert.That(page.styleTriggerWithDynamicResources.BackgroundColor, Is.EqualTo(Colors.Green));

			}
		}
	}
}