using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1734, "Add support for Label line height", PlatformAffected.All)]
	public class Issue1734
		: TestContentPage
	{
		Label _label;
		Label _spannedLabel;

		protected override void Init()
		{
			var text = "This is a very long text that hopefully spans over many many many many multiple beautiful lines!";

			_label = new Label { FormattedText = text, LineBreakMode = LineBreakMode.WordWrap, BackgroundColor = Colors.AliceBlue, TextColor = Colors.DarkGray };
			var lineHeight = new Entry();
			lineHeight.TextChanged += LineHeightChanged;

			FormattedString s = new FormattedString();
			s.Spans.Add(new Span { Text = "0.8: This is a\nvery long text\n", LineHeight = 0.8, BackgroundColor = Colors.BlueViolet });
			s.Spans.Add(new Span { Text = "1.2: with multiple\nlines abc\n", LineHeight = 1.2, BackgroundColor = Colors.Brown });
			s.Spans.Add(new Span { Text = "0.2: def\ndef\n", LineHeight = 0.2, BackgroundColor = Colors.Pink });
			s.Spans.Add(new Span { Text = "1.5: ghi jkl\nmno\npqr stu vwx yz ", LineHeight = 1.5, BackgroundColor = Colors.HotPink });
			_spannedLabel = new Label { FormattedText = s, LineBreakMode = LineBreakMode.WordWrap };

			var stackLayout = new StackLayout
			{
				Children = { new Label() { Text = "Line height" }, lineHeight, _label, _spannedLabel },
				Padding = new Thickness() { Top = 20, Left = 10, Right = 100 }
			};

			Content = new ScrollView { Content = stackLayout };
		}

		private void LineHeightChanged(object sender, TextChangedEventArgs e)
		{
			var nr = ((Entry)sender).Text;
			try
			{
				_label.LineHeight = double.Parse(nr);
			}
			catch (FormatException fe)
			{
				Debug.WriteLine($"Could not parse {nr}. {fe.Message}");
			}
		}
	}
}
