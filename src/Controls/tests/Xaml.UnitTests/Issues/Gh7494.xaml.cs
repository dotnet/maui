// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7494 : ContentPage
{
	public Gh7494() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void TemplateBindingInSpans(XamlInflator inflator)
		{
			var layout = new Gh7494(inflator);
			var view = layout.Content as Gh7494Content;
			var templatedLabel = ((StackLayout)(view as IVisualTreeElement).GetVisualChildren()[0]).Children[0] as Label;

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
