// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Maui;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Gh7494 : ContentPage
	{
		public Gh7494() => InitializeComponent();
		public Gh7494(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void TemplateBindingInSpans([Values(false, true)]bool useCompiledXaml)
			{
				var layout = new Gh7494(useCompiledXaml);
				var view = layout.Content as Gh7494Content;
				var templatedLabel = ((StackLayout)view.Children[0]).Children[0] as Label;

				Assert.That(templatedLabel.FormattedText.Spans[0].Text, Is.EqualTo(view.Title));
			}
		}
	}

	public class Gh7494Content : ContentView
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Gh7494Content), default(string));

		public string Title {
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}
	}
}
