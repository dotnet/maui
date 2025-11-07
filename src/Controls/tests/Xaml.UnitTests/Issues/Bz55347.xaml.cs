using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz55347 : ContentPage
{
	public Bz55347()
	{
	}


	public class Tests : IDisposable
	{
		public void Dispose()
		{
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void PaddingThicknessResource(XamlInflator inflator)
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary {
					{"Padding", new Thickness(8)}
				}
			};
			var layout = new Bz55347(inflator);
		}
	}
}