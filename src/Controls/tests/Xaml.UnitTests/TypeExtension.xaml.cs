using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
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

		public ICommand ProvideValue(IServiceProvider serviceProvider)
		{
			return new Command(() => { });
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return ProvideValue(serviceProvider);
		}
	}

	public partial class TypeExtension : ContentPage
	{
		public TypeExtension()
		{
			InitializeComponent();
		}

		public TypeExtension(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void NestedMarkupExtensionInsideDataTemplate(bool useCompiledXaml)
			{
				var page = new TypeExtension(useCompiledXaml);
				var listView = page.listview;
				listView.ItemsSource = new string[2];

				var cell = (ViewCell)listView.TemplatedItems[0];
				var button = (Button)cell.View;
				Assert.NotNull(button.Command);

				cell = (ViewCell)listView.TemplatedItems[1];
				button = (Button)cell.View;
				Assert.NotNull(button.Command);
			}

			[Xunit.Theory]
			[InlineData(false)]
			[InlineData(true)]
			//https://bugzilla.xamarin.com/show_bug.cgi?id=55027
			public void TypeExtensionSupportsNamespace(bool useCompiledXaml)
			{
				var page = new TypeExtension(useCompiledXaml);
				var button = page.button0;
				Assert.Equal(typeof(TypeExtension), button.CommandParameter);
			}
		}
	}
}