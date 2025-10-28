using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21839
{
	public Maui21839()
	{
		InitializeComponent();
	}

	public Maui21839(bool useCompiledXaml)
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

		[Fact]
		public async Task VSMLeak([InlineData(false, true)] bool useCompiledXaml)
		{
			Application.Current.Resources.Add("buttonStyle",
				new Style(typeof(Button))
				{
					Setters = {
						new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = new VisualStateGroupList{
							new VisualStateGroup {
								Name = "CommonStates",
								States = {
									new VisualState { Name = "Normal" },
									new VisualState { Name = "Pressed" },
									new VisualState { Name = "Disabled" }
								}
							}
						} }
					}
				});
			var pagewr = new WeakReference(new Maui21839(useCompiledXaml));
			await Task.Delay(10);
			GC.Collect();
			Assert.Null(pagewr.Target, "Page leaked");
		}
	}
}
