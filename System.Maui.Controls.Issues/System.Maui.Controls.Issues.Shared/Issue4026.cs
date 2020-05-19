using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
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
			BackgroundColor = Color.Aquamarine;
			var btn = new Button { Text = "Add More Text to Span", Command = new Command(() => Title += " More Text is here") };
			var spanBindable = new Span { TextColor = Color.Blue, };
			spanBindable.SetBinding(Span.TextProperty, new Binding(nameof(Title), BindingMode.OneWay));
			var label = new Label { LineBreakMode = LineBreakMode.WordWrap, VerticalTextAlignment = TextAlignment.Center, BackgroundColor = Color.Red, FormattedText = new FormattedString { Spans = { new Span { Text = "Span Test Span Test Span Test Span Test" }, spanBindable } } };
			BindingContext = this;
			Content = new StackLayout { VerticalOptions = LayoutOptions.Start, BackgroundColor = Color.CadetBlue, Padding = new Thickness(10), Children = { new Label { Text = "When you add new spans, all of them should appear, they should not be cut." }, btn, label } };
		}
	}
}
