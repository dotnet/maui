using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz56852
{
	public Bz56852()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{


		public void Dispose() { }
		[Theory]
		[Values]
		public void DynamicResourceApplyingOrder(XamlInflator inflator)
		{
			var layout = new Bz56852(inflator);
			Assert.Equal(50, layout.label.FontSize);
		}
	}
}
