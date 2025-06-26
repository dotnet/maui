using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20508
{
	public Maui20508()
	{
		InitializeComponent();
	}

	public Maui20508(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		// Constructor
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void ToolBarItemBinding([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{
			var page = new Maui20508(useCompiledXaml) { BindingContext = new { Icon = "boundIcon.png" } };
			Assert.Equal("boundIcon.png", ((FileImageSource)page.ToolbarItems[0].IconImageSource).File);
		}
	}
}
