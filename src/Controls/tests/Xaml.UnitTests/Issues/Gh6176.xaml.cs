using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh6176VM
{
}

public class Gh6176Base<TVM> : ContentPage where TVM : class
{
	public TVM ViewModel => BindingContext as TVM;
	protected void ShowMenu(object sender, EventArgs e) { }
}

public partial class Gh6176
{
	public Gh6176() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void XamlCDoesntFail(XamlInflator inflator)
		{
			var layout = new Gh6176(inflator);
		}
	}
}
