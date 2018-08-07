using System;
using System.Linq.Expressions;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3001, "[macOS] Navigating back from a complex page is highly inefficient", PlatformAffected.macOS)]
	public class Issue3001 : TestContentPage
	{
		const string ButtonId = "ClearButton";
		const string ReadyId = "ReadyLabel";

		int _counter = 0;
		int _level = 0;
		const int maxLevel = 5;

		public View BuildLevel()
		{
			if (_level == maxLevel)
			{
				_counter++;
				return new Label { Text = _counter.ToString(), FontSize = 10 };
			}

			_level++;
			var g1 = new Grid { RowSpacing = 0, ColumnSpacing = 0 };

			var g2 = new Grid { RowSpacing = 0, ColumnSpacing = 0 };
			g1.Children.Add(g2);

			var g = new Grid { RowSpacing = 0, ColumnSpacing = 0 };
			g2.Children.Add(g);

			g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
			g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

			g.Children.Add(BuildLevel(), 0, 0);
			g.Children.Add(BuildLevel(), 0, 1);
			g.Children.Add(BuildLevel(), 1, 0);
			g.Children.Add(BuildLevel(), 1, 1);

			_level--;

			return g1;
		}

		protected override void Init()
		{
			var sp = new StackLayout();
			sp.Children.Add(new Button
			{
				Text = "Start",
				AutomationId = ButtonId,
				Command = new Command(() =>
				{
					Content = new Label
					{
						Text = "Ready",
						AutomationId = ReadyId
					};
				})
			});
			sp.Children.Add(BuildLevel());
			Content = sp;
		}

#if UITEST
#if __MACOS__
		[Test]
		public void Issue3001Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ButtonId));
			RunningApp.Tap(q => q.Marked(ButtonId));
			RunningApp.WaitForElement(q => q.Marked(ReadyId), timeout: TimeSpan.FromSeconds(5));
		}
#endif
#endif
	}
}
