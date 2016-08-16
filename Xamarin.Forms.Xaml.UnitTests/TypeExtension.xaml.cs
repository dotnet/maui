using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public enum NavigationOperation
	{
		Forward,
		Back,
		Replace,
	}

	[ContentProperty(nameof(Operation))]
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

	public partial class TypeExtension : ListView
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
			[TestCase(false)]
			[TestCase(true)]
			public void NestedMarkupExtensionInsideDataTemplate(bool useCompiledXaml)
			{
				var listView = new TypeExtension(useCompiledXaml);
				listView.ItemsSource = new string [2];

				var cell = (ViewCell)listView.TemplatedItems [0];
				var button = (Button)cell.View;
				Assert.IsNotNull(button.Command);

				cell = (ViewCell)listView.TemplatedItems [1];
				button = (Button)cell.View;
				Assert.IsNotNull(button.Command);
			}
		}
	}
}