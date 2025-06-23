using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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
		public void ToolBarItemBinding([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui20508(useCompiledXaml) { BindingContext = new { Icon = "boundIcon.png" } };
			Assert.That(((FileImageSource)page.ToolbarItems[0].IconImageSource).File, Is.EqualTo("boundIcon.png"));
		}
	}
}
