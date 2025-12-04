using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20616
{
	public Maui20616() => InitializeComponent();

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
		public void XDataTypeCanBeGeneric([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui20616));

			}
			var page = new Maui20616(inflator) { BindingContext = new ViewModel20616<string> { Value = "Foo" } };

			page.LabelA.BindingContext = new ViewModel20616<string> { Value = "ABC" };
			Assert.AreEqual("ABC", page.LabelA.Text);

			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
			{
				var binding = page.LabelA.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(binding, Is.TypeOf<TypedBinding<ViewModel20616<string>, string>>());
			}

			page.LabelB.BindingContext = new ViewModel20616<ViewModel20616<bool>> { Value = new ViewModel20616<bool> { Value = true } };
			Assert.AreEqual("True", page.LabelB.Text);

			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
			{
				var binding = page.LabelB.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(binding, Is.TypeOf<TypedBinding<ViewModel20616<ViewModel20616<bool>>, bool>>());
			}

			Assert.AreEqual(typeof(ViewModel20616<bool>), page.Resources["ViewModelBool"]);
			Assert.AreEqual(typeof(ViewModel20616<ViewModel20616<string>>), page.Resources["NestedViewModel"]);
		}
	}
}

public class ViewModel20616<T>
{
	public required T Value { get; init; }
}