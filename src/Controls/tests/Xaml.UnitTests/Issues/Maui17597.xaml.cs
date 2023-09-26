using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Microsoft.Maui.Dispatching;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17597 : ContentPage
{

	public Maui17597() => InitializeComponent();

	public Maui17597(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);
		[Test]
		public void DataTriggerInStyle([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui17597(useCompiledXaml);
			Assert.That(page.Test_Entry.Text, Is.EqualTo("Remove Text To Disable Button"));
			Assert.That(page.button.IsEnabled, Is.True);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "");
			Assert.That(page.Test_Entry.Text, Is.Empty);
			Assert.That(page.Test_Entry.Text.Length, Is.EqualTo(0));
			Assert.That(page.button.IsEnabled, Is.False);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "foo");
			Assert.That(page.Test_Entry.Text, Is.Not.Empty);
			Assert.That(page.button.IsEnabled, Is.True);
		}
	}
}