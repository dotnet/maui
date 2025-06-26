// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh7494 : ContentPage
	{
		public Gh7494() => InitializeComponent();
		public Gh7494(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			public void TemplateBindingInSpans([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				var layout = new Gh7494(useCompiledXaml);
				var view = layout.Content as Gh7494Content;
				var templatedLabel = ((StackLayout)view.Children[0]).Children[0] as Label;

				Assert.Equal(view.Title, templatedLabel.FormattedText.Spans[0].Text);
			}
		}
	}

	public class Gh7494Content : ContentView
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Gh7494Content), default(string));

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}
	}
}
