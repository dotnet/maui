using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25935
{
	public Maui25935() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void ToolBarItemAppThemeBinding(XamlInflator inflator)
		{
			var page = new Maui25935(inflator);
			var items = page.Picker.Items.ToArray();
			Assert.Contains("1", items);
			Assert.Contains("2", items);
			Assert.Contains("3", items);
		}
	}
}
