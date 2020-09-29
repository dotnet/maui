using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void xArrayCanBeAssignedToIListT(bool useCompiledXaml)
			{
				var layout = new Bz36422(useCompiledXaml);
				Assert.AreEqual(3, layout.control.Views.Count);
			}
		}
	}
}