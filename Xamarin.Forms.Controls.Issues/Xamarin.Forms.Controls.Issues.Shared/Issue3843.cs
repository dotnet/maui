using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3843, "[Enhancement] Hide Scroll Bars on ListView")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue3843 : TestContentPage
	{
		protected override void Init()
		{
			ListView list = new ListView();
			list.BackgroundColor = Color.Yellow;
			list.ItemsSource = 
				Enumerable
				.Range(0, 1000)
				.Select(x=> String.Join("", Enumerable.Range(0,100))  )
				.ToArray();

			list.ItemTemplate = new DataTemplate(() =>
			{
				ViewCell cell = new ViewCell();

				Label label = new Label();
				label.LineBreakMode = LineBreakMode.NoWrap;
				label.SetBinding(Label.TextProperty, ".");

				cell.View = label;
				return cell;
			});

			Label labelScrollBarState = new Label();
			Content = new StackLayout()
			{
				Orientation = StackOrientation.Vertical,
				Children =
				{
					new Label()
					{
						Text = "Click the buttons to toggle scrollbar visibility and validate they works"
					},
					labelScrollBarState,
					list,
					new Button()
					{
						Text = "Toggle Horizontal",
						Command = new Command(() =>
						{
							switch(list.HorizontalScrollBarVisibility)
							{
								case ScrollBarVisibility.Always:
									list.HorizontalScrollBarVisibility = ScrollBarVisibility.Default;
								break;
								case ScrollBarVisibility.Default:
									list.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
								break;
								case ScrollBarVisibility.Never:
									list.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
								break;
							}
							UpdateScrollVisibility(list, labelScrollBarState);
						})
					},
					new Button()
					{
						Text = "Toggle Vertical",
						Command = new Command(() =>
						{
							switch(list.VerticalScrollBarVisibility)
							{
								case ScrollBarVisibility.Always:
									list.VerticalScrollBarVisibility = ScrollBarVisibility.Default;
								break;
								case ScrollBarVisibility.Default:
									list.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
								break;
								case ScrollBarVisibility.Never:
									list.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
								break;
							}
							UpdateScrollVisibility(list, labelScrollBarState);
						})
					},
					
				}
			};

			UpdateScrollVisibility(list, labelScrollBarState);
		}

		void UpdateScrollVisibility(ListView listView, Label label)
		{
			label.Text = $"Horizontal: {listView.HorizontalScrollBarVisibility} Vertical: {listView.VerticalScrollBarVisibility}";
		}

	}
}
