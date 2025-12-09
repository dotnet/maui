using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21757_2
{
	public Maui21757_2() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void TypeLiteralAndXTypeCanBeUsedInterchangeably(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui21757_2));
				Assert.Empty(result.Diagnostics);
			}
			else if (inflator == XamlInflator.XamlC)
			{
				var ex = Record.Exception(() => MockCompiler.Compile(typeof(Maui21757_2)));
				Assert.Null(ex);
			}
		}
	}
}

public class ViewModelMainPage21757_2
{
	public List<ViewModelTest21757_2> TestList { get; set; }

	public ViewModelMainPage21757_2()
	{
		TestList =
		[
			new ViewModelTest21757_2() { TestValue = 0 },
			new ViewModelTest21757_2() { TestValue = 1 },
			new ViewModelTest21757_2() { TestValue = 2 },
			new ViewModelTest21757_2() { TestValue = 3 }
		];
	}
}

public class ViewModelTest21757_2
{
	public int TestValue { get; set; }
}