using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21839
{
	public Maui21839() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public async Task VSMLeak(XamlInflator inflator)
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
			Assert.Null(pagewr.Target);
		}
	}
}
