namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

public partial class Maui29334 : ContentPage
{

	public Maui29334() => InitializeComponent();

	public Maui29334(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void OnIdiomGridLength([Values] bool useCompiledXaml)
		{
			var page = new Maui29334(useCompiledXaml);
			
		}
	}
}