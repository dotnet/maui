using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class EntryPointNotFound : ContentPage
{
	public EntryPointNotFound() => InitializeComponent();
	public EntryPointNotFound(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void XSharedIsSupportedOnResources([Values(false, true)]bool useCompiledXaml)
		{
			var layout = new EntryPointNotFound(useCompiledXaml);
		}
	}
}