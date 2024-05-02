using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18103 : ContentPage
{

	public Maui18103() => InitializeComponent();

	public Maui18103(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void VSMOverride([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui18103(useCompiledXaml);
			Assert.That(page.button.Background, Is.EqualTo(new SolidColorBrush(Colors.Orange)));
		}
	}
}