using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

public partial class Maui20818
{
	public Maui20818()
	{
		InitializeComponent();
	}

	public Maui20818(bool useCompiledXaml)
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
			var page = new Maui20818(useCompiledXaml);

			Assert.Equal(typeof(Label), (page.Resources["A"] as Style).TargetType);
			Assert.Equal(typeof(Label), (page.Resources["B"] as Style).TargetType);

			Assert.Equal(typeof(Label), page.TriggerC.TargetType);
			Assert.Equal(typeof(Label), page.TriggerD.TargetType);
			Assert.Equal(typeof(Label), page.TriggerE.TargetType);
			Assert.Equal(typeof(Label), page.TriggerF.TargetType);
			Assert.Equal(typeof(Label), page.TriggerG.TargetType);
			Assert.Equal(typeof(Label), page.TriggerH.TargetType);
		}
	}
}
