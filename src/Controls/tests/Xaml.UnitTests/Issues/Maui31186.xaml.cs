using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class MyButton : Button
{

}

public partial class Maui31186 : ContentPage
{
	public Maui31186()
	{
		InitializeComponent();
	}
	int count = 0;

	void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";
	}


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void XmlnsResolutionForVisualState(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
						.WithAdditionalSource(
	"""
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class MyButton : Button
{

}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui31186 : ContentPage
{
	public Maui31186()
	{
		InitializeComponent();
	}
	int count = 0;

	void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";
	}
}
""")
						.RunMauiSourceGenerator(typeof(Maui31186));
				Assert.Empty(result.Diagnostics);
			}
			var page = new Maui31186(inflator);
			Assert.NotNull(page);
			VisualStateManager.GoToState(page.CounterBtn, "Disabled");
			Assert.Equal(Colors.LightBlue, page.CounterBtn.BackgroundColor);
		}
	}
}