using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui21757_2
{
	public Maui21757_2()
	{
		InitializeComponent();
	}

	public Maui21757_2(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Fact]
		public void TypeLiteralAndXTypeCanBeUsedInterchangeably()
		{
			Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui21757_2)));
		}
	}
}

public class ViewModelMainPage21757_2
{
	public List<ViewModelTest21757_2> TestList { get; set; }

	public ViewModelMainPage21757_2()
	{
		TestList = new List<ViewModelTest21757_2>()
		{
			new ViewModelTest21757_2() { TestValue = 0 },
			new ViewModelTest21757_2() { TestValue = 1 },
			new ViewModelTest21757_2() { TestValue = 2 },
			new ViewModelTest21757_2() { TestValue = 3 }
		};
	}
}

public class ViewModelTest21757_2
{
	public int TestValue { get; set; }
}