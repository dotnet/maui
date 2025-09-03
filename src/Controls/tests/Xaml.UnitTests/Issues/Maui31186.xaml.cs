using System;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void XmlnsResolutionForVisualState([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
						.WithAdditionalSource(
	"""
using System;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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
				Assert.That(result.Diagnostics, Is.Empty);
			}
			var page = new Maui31186(inflator);
			Assert.That(page, Is.Not.Null);
			VisualStateManager.GoToState(page.CounterBtn, "Disabled");
			Assert.That(page.CounterBtn.BackgroundColor, Is.EqualTo(Colors.LightBlue));
		}
	}
}