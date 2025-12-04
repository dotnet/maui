using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21757_2
{
	public Maui21757_2() => InitializeComponent();

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
		public void TypeLiteralAndXTypeCanBeUsedInterchangeably([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui21757_2));
				Assert.That(result.Diagnostics, Is.Empty);
			}
			else if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui21757_2)));
			else
				Assert.Ignore("Only XamlC and SourceGen support this feature");
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