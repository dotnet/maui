using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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

		[TestFixture]
		public class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[TestCase(false)]
			[TestCase(true)]
			public void NestedMarkupExtensionInsideDataTemplate(bool useCompiledXaml)
			{
				var page = new TypeExtension(useCompiledXaml);
				var listView = page.listview;
				listView.ItemsSource = new string[2];

				var cell = (ViewCell)listView.TemplatedItems[0];
				var button = (Button)cell.View;
				Assert.IsNotNull(button.Command);

				cell = (ViewCell)listView.TemplatedItems[1];
				button = (Button)cell.View;
				Assert.IsNotNull(button.Command);
			}

			[TestCase(false)]
			[TestCase(true)]
			//https://bugzilla.xamarin.com/show_bug.cgi?id=55027
			public void TypeExtensionSupportsNamespace(bool useCompiledXaml)
			{
				var page = new TypeExtension(useCompiledXaml);
				var button = page.button0;
				Assert.That(button.CommandParameter, Is.EqualTo(typeof(TypeExtension)));
			}
		}
	}
}