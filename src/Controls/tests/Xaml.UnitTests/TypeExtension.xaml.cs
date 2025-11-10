using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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

	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void NestedMarkupExtensionInsideDataTemplate(XamlInflator inflator)
		{
			var page = new TypeExtension(inflator);
			var listView = page.listview;
			listView.ItemsSource = new string[2];

			var cell = (ViewCell)listView.TemplatedItems[0];
			var button = (Button)cell.View;
			Assert.NotNull(button.Command);

			cell = (ViewCell)listView.TemplatedItems[1];
			button = (Button)cell.View;
			Assert.NotNull(button.Command);
		}

		[Theory]
		[Values]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=55027
		public void TypeExtensionSupportsNamespace(XamlInflator inflator)
		{
			var page = new TypeExtension(inflator);
			var button = page.button0;
			Assert.Equal(typeof(TypeExtension), button.CommandParameter);
		}

		[Fact]
		public void ExtensionsAreReplaced() // TODO: Fix parameters - see comment above] XamlInflator inflator)
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using System;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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
			Assert.False(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			Assert.True(initComp.Contains("typeof(global::Microsoft.Maui.Controls.Xaml.UnitTests.TypeExtension)", StringComparison.InvariantCulture));
		}
	}
}