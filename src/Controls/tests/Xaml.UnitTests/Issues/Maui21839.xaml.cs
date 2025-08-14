using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21839
{
	public Maui21839() => InitializeComponent();

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
		public async Task VSMLeak([Values] XamlInflator inflator)
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
			var pagewr = new WeakReference(new Maui21839(inflator));
			await Task.Delay(10);
			GC.Collect();
			Assert.IsNull(pagewr.Target, "Page leaked");
		}
	}
}
