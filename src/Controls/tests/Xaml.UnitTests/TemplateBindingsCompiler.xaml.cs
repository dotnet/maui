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
using Xunit;

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
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void Test(bool useCompiledXaml)
			{
				var page = new TemplateBindingsCompiler(useCompiledXaml);
				var label = (Label)page.ContentView.GetTemplateChild("CardTitleLabel");
				Assert.Equal("The title", label?.Text);

				if (useCompiledXaml)
				{
					var binding = label.GetContext(Label.TextProperty).Bindings.GetValue();
					Assert.True(binding, Is.TypeOf<TypedBinding<TemplateBindingCompilerTestCardView, string>>());
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