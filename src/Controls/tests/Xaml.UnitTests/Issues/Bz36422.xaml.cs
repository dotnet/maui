using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz36422Control : ContentView
	{
		public IList<ContentView> Views { get; set; }
	}

	public partial class Bz36422 : ContentPage
	{
		public Bz36422()
		{
			InitializeComponent();
		}


		public class Tests
		{
			[Theory]
			[Values]
			public void xArrayCanBeAssignedToIListT(XamlInflator inflator)
			{
				var layout = new Bz36422(inflator);
				Assert.Equal(3, layout.control.Views.Count);
			}
		}
	}
}