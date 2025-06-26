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

		public Bz36422(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void xArrayCanBeAssignedToIListT(bool useCompiledXaml)
			{
				var layout = new Bz36422(useCompiledXaml);
				Assert.Equal(3, layout.control.Views.Count);
			}
		}
	}
}