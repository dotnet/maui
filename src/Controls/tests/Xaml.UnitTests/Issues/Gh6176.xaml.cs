using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh6176VM
{
}

public class Gh6176Base<TVM> : ContentPage where TVM : class
{
	public TVM ViewModel => BindingContext as TVM;
	protected void ShowMenu(object sender, EventArgs e) { }
}

[XamlProcessing(XamlInflator.Default, true)]
public partial class Gh6176
{
	public Gh6176() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void XamlCDoesntFail([Values] XamlInflator inflator)
		{
			var layout = new Gh6176(inflator);
		}
	}
}
