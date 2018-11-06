using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 28650, "In a Listview on iOS, \"andExpand\" does not expand when text is two lines long")]
	public class Bugzilla28650 : TestContentPage
	{
		const string caret_image = "caret_r.png";

		[Preserve(AllMembers = true)]
		internal class MyTextCell : ViewCell
		{
			StackLayout _viewLayout;
			Label _descriptionLabel;
			Image _caret;

			public MyTextCell()
			{
				_descriptionLabel = new Label
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					FontSize = 14,
				};
				_descriptionLabel.SetBinding(Label.TextProperty, ".");

				_caret = new Image
				{
					HorizontalOptions = LayoutOptions.End,
					Source = ImageSource.FromFile(caret_image),
					HeightRequest = 20,
					Aspect = Aspect.AspectFit,
					BackgroundColor = Color.Green
				};

				_viewLayout = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = { _descriptionLabel, _caret },
				};

				View = _viewLayout;
			}
		}

		protected override void Init()
		{
			var items = new List<string>
			{
				"Short Desc",
				"Thisis averylong description withwords thatarelong",
				"Item Three",
				"Item Four",
				"Short Desc again",
			};

			var cell = new DataTemplate(typeof(MyTextCell));
			var menuView = new ListView
			{
				ItemTemplate = cell,
				ItemsSource = items,
				HasUnevenRows = true
			};

			// let's try the same configuration outside of a ListView
			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 300 });

			const int column = 0;
			int currentRow = 0;
			grid.AddChild(new Label { Text = "If the carets do not ALL align, this test has failed." }, column, currentRow++, columnspan: 2);
			grid.AddChild(menuView, column, currentRow++);
			grid.AddChild(GetStackLayout("Thisis averylong description withwords thatarelong", LineBreakMode.NoWrap), column, currentRow++);
			grid.AddChild(GetStackLayout("Thisis averylong description withwords thatarelong", LineBreakMode.CharacterWrap), column, currentRow++);
			grid.AddChild(GetStackLayout("Thisis averylong description withwords thatarelong", LineBreakMode.HeadTruncation), column, currentRow++);
			grid.AddChild(GetStackLayout("Thisis averylong description withwords thatarelong", LineBreakMode.MiddleTruncation), column, currentRow++);
			grid.AddChild(GetStackLayout("Thisis averylong description withwords thatarelong", LineBreakMode.TailTruncation), column, currentRow++);
			grid.AddChild(GetStackLayout("Thisis averylong description withwords thatarelong", LineBreakMode.WordWrap), column, currentRow++);
			grid.AddChild(GetStackLayout("Short desc", LineBreakMode.WordWrap), column, currentRow++);


			grid.AddChild(new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						FontSize = 14,
						Text = "1",
						LineBreakMode = LineBreakMode.WordWrap,
						BackgroundColor = Color.Red
					}, new Label
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						FontSize = 14,
						Text = "2",
						LineBreakMode = LineBreakMode.WordWrap,
						BackgroundColor = Color.Blue
					}
				},
			}, column, currentRow++);

			grid.AddChild(new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						FontSize = 14,
						Text = "1",
						LineBreakMode = LineBreakMode.WordWrap,
						BackgroundColor = Color.Red
					}, new Label
					{
						HorizontalOptions = LayoutOptions.Start,
						FontSize = 14,
						Text = "2",
						LineBreakMode = LineBreakMode.WordWrap,
						BackgroundColor = Color.Blue
					}
				},
			}, column, currentRow++);

			Content = grid;
		}
		private static StackLayout GetStackLayout(string Text, LineBreakMode breakMode)
		{
			return new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						FontSize = 14,
						Text = Text,
						LineBreakMode = breakMode
					}, new Image
					{
						HorizontalOptions = LayoutOptions.End,
						Source = ImageSource.FromFile(caret_image),
						HeightRequest = 20,
						Aspect = Aspect.AspectFit,
						BackgroundColor = Color.Green
					}
				},
			};
		}
	}
}
