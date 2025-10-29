using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17597 : ContentPage
{

	public Maui17597() => InitializeComponent();

	public Maui17597(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);
		[Theory]
			public void Method(bool useCompiledXaml)
		{
			var page = new Maui17597(useCompiledXaml);
			Assert.Equal("Remove Text To Disable Button", page.Test_Entry.Text);
			Assert.True(page.button.IsEnabled, Is.True);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "");
			Assert.True(page.Test_Entry.Text, Is.Empty);
			Assert.Equal(0, page.Test_Entry.Text.Length);
			Assert.True(page.button.IsEnabled, Is.False);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "foo");
			Assert.True(page.Test_Entry.Text, Is.Not.Empty);
			Assert.True(page.button.IsEnabled, Is.True);
		}
	}
}