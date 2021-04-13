using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4026, "Bindable Span height Issue", PlatformAffected.All)]
	public class Issue4026 : TestContentPage
	{
		protected override void Init()
		{
			Padding = new Thickness(0, 40, 0, 0);
			BackgroundColor = Colors.Aquamarine;
			var btn = new Button { Text = "Add More Text to Span", Command = new Command(() => Title += " More Text is here") };
			var spanBindable = new Span { TextColor = Colors.Blue, };
			spanBindable.SetBinding(Span.TextProperty, new Binding(nameof(Title), BindingMode.OneWay));
			var label = new Label { LineBreakMode = LineBreakMode.WordWrap, VerticalTextAlignment = TextAlignment.Center, BackgroundColor = Colors.Red, FormattedText = new FormattedString { Spans = { new Span { Text = "Span Test Span Test Span Test Span Test" }, spanBindable } } };
			BindingContext = this;
			Content = new StackLayout { VerticalOptions = LayoutOptions.Start, BackgroundColor = Colors.CadetBlue, Padding = new Thickness(10), Children = { new Label { Text = "When you add new spans, all of them should appear, they should not be cut." }, btn, label } };
		}
	}
}
