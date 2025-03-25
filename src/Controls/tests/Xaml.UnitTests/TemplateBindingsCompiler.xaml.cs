using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class TemplateBindingsCompiler : ContentPage
	{
		public TemplateBindingsCompiler()
		{
			InitializeComponent();
		}

		public TemplateBindingsCompiler(bool useCompiledXaml)
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
			public void Test(bool useCompiledXaml)
			{
				var page = new TemplateBindingsCompiler(useCompiledXaml);
				var label = (Label)page.ContentView.GetTemplateChild("CardTitleLabel");
				Assert.AreEqual("The title", label?.Text);

				if (useCompiledXaml)
				{
					var binding = label.GetContext(Label.TextProperty).Bindings.GetValue();
					Assert.That(binding, Is.TypeOf<TypedBinding<TemplateBindingCompilerTestCardView, string>>());
				}
			}
		}
	}

	public class TemplateBindingCompilerTestCardView : ContentView
	{
		public static readonly BindableProperty CardTitleProperty =
			BindableProperty.Create(nameof(CardTitle), typeof(string), typeof(TemplateBindingCompilerTestCardView), string.Empty);

		public string CardTitle
		{
			get => (string)GetValue(CardTitleProperty);
			set => SetValue(CardTitleProperty, value);
		}
	}
}