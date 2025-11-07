using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz54717 : ContentPage
{
	public Bz54717()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{


		public void Dispose() { }
		[Theory]
		[Values]
		public void FooBz54717(XamlInflator inflator)
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary {
					{"Color1", Colors.Red},
					{"Color2", Colors.Blue},
				}
			};
			var layout = new Bz54717(inflator);
			Assert.Single(layout.Resources);
			var array = layout.Resources["SomeColors"] as Color[];
			Assert.Equal(Colors.Red, array[0]);
			Assert.Equal(Colors.Blue, array[1]);
		}
	}
}
