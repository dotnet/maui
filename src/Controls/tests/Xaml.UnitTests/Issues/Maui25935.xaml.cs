using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25935
{
	public Maui25935()
	{
		InitializeComponent();
	}

	public Maui25935(bool useCompiledXaml)
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
		public void ToolBarItemAppThemeBinding([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui25935(useCompiledXaml);
			var items = page.Picker.Items.ToArray();
			Assert.Contains("1", items);
			Assert.Contains("2", items);
			Assert.Contains("3", items);
		}
	}
}