using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

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

		[TestFixture]
		class Tests
		{
			[Test]
			public void xArrayCanBeAssignedToIListT([Values] XamlInflator inflator)
			{
				var layout = new Bz36422(inflator);
				Assert.AreEqual(3, layout.control.Views.Count);
			}
		}
	}
}