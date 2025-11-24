using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
public enum NavigationOperation
{
	Forward,
	Back,
	Replace,
}

[ContentProperty(nameof(Operation))]
[AcceptEmptyServiceProvider]
public class NavigateExtension : IMarkupExtension<ICommand>
{
	public NavigationOperation Operation { get; set; }

	public Type Type { get; set; }

	public ICommand ProvideValue(IServiceProvider serviceProvider) => new Command(() => { });

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

public partial class TypeExtension : ContentPage
{
	public TypeExtension() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void NestedMarkupExtensionInsideDataTemplate([Values] XamlInflator inflator)
		{
			var page = new TypeExtension(inflator);
			var listView = page.listview;
			listView.ItemsSource = new string[2];

			var cell = (ViewCell)listView.TemplatedItems[0];
			var button = (Button)cell.View;
			Assert.IsNotNull(button.Command);

			cell = (ViewCell)listView.TemplatedItems[1];
			button = (Button)cell.View;
			Assert.IsNotNull(button.Command);
		}

		[Test]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=55027
		public void TypeExtensionSupportsNamespace([Values] XamlInflator inflator)
		{
			var page = new TypeExtension(inflator);
			var button = page.button0;
			Assert.That(button.CommandParameter, Is.EqualTo(typeof(TypeExtension)));
		}

		[Test]
		public void ExtensionsAreReplaced([Values(XamlInflator.SourceGen)] XamlInflator inflator)
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using System;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
public enum NavigationOperation
{
	Forward,
	Back,
	Replace,
}

[ContentProperty(nameof(Operation))]
[AcceptEmptyServiceProvider]
public class NavigateExtension : IMarkupExtension<ICommand>
{
	public NavigationOperation Operation { get; set; }

	public Type Type { get; set; }

	public ICommand ProvideValue(IServiceProvider serviceProvider) => new Command(() => { });

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

public partial class TypeExtension : ContentPage
{
	public TypeExtension() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(TypeExtension));
			Assert.IsFalse(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			Assert.That(initComp.Contains("typeof(global::Microsoft.Maui.Controls.Xaml.UnitTests.TypeExtension)", StringComparison.InvariantCulture));
		}
	}
}