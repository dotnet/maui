using System.Globalization;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1733, "Autoresizable Editor")]
	public class Issue1733 : TestContentPage
	{
		public const string editorHeightShrinkWithPressureId = "editorHeightShrinkWithPressureId";
		public const string editorHeightGrowId = "editorHeightGrowId";
		public const string editorWidthGrow1Id = "editorWidthGrow1Id";
		public const string editorWidthGrow2Id = "editorWidthGrow2Id";
		public const string btnChangeFontToDefault = "Change the Font to Default";
		public const string btnChangeFontToLarger = "Change the Font to Larger";
		public const string btnChangeToHasText = "Change to Has Text";
		public const string btnChangeToNoText = "Change to Has No Text";
		public const string btnChangeSizeOption = "Change the Size Option";

		protected override void Init()
		{
			StackLayout container = new StackLayout()
			{
				BackgroundColor = Colors.Purple
			};

			StackLayout layout = new StackLayout()
			{
				BackgroundColor = Colors.Pink,
				HeightRequest = 200
			};

			var editor = new Editor()
			{
				BackgroundColor = Colors.Green,
				MinimumHeightRequest = 10,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorHeightShrinkWithPressureId
			};

			var editorHeightGrow = new Editor()
			{
				BackgroundColor = Colors.Green,
				MinimumHeightRequest = 200,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorHeightGrowId
			};


			layout.Children.Add(editor);
			layout.Children.Add(editorHeightGrow);

			StackLayout layoutHorizontal = new StackLayout()
			{
				BackgroundColor = Colors.Yellow,
				Orientation = StackOrientation.Horizontal
			};

			var editorWidthGrow1 = new Editor()
			{
				BackgroundColor = Colors.Green,
				MinimumWidthRequest = 10,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorWidthGrow1Id
			};

			var editorWidthGrow2 = new Editor()
			{
				BackgroundColor = Colors.Green,
				MinimumWidthRequest = 200,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorWidthGrow2Id,
				ClassId = editorWidthGrow2Id
			};


			layoutHorizontal.Children.Add(editorWidthGrow1);
			layoutHorizontal.Children.Add(editorWidthGrow2);

			container.Children.Add(layout);
			container.Children.Add(layoutHorizontal);


			List<Editor> editors = new List<Editor>()
			{
				editor, editorHeightGrow, editorWidthGrow1, editorWidthGrow2
			};

			Button buttonChangeFont = new Button()
			{
				AutomationId = btnChangeFontToLarger,
				Text = btnChangeFontToLarger
			};


			Button buttonChangeText = new Button()
			{
				AutomationId = btnChangeToHasText,
				Text = btnChangeToHasText
			};

			Button buttonChangeSizeOption = new Button()
			{
				AutomationId = btnChangeSizeOption,
				Text = btnChangeSizeOption
			};

			double fontSizeInitial = editor.FontSize;
			buttonChangeFont.Clicked += (x, y) =>
			{
				editors.ForEach(e =>
				{
					if (e.FontSize == fontSizeInitial)
					{
						e.FontSize = 40;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
						Device.BeginInvokeOnMainThread(() => buttonChangeFont.Text = btnChangeFontToDefault);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
					}
					else
					{
						e.FontSize = fontSizeInitial;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
						Device.BeginInvokeOnMainThread(() => buttonChangeFont.Text = btnChangeFontToLarger);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
					}
				});
			};

			buttonChangeText.Clicked += (_, __) =>
			{
				editors.ForEach(e =>
				{
					if (String.IsNullOrWhiteSpace(e.Text))
					{
						e.Text = String.Join(" ", Enumerable.Range(0, 100).Select(x => "f").ToArray());
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
						Device.BeginInvokeOnMainThread(() => buttonChangeText.Text = btnChangeToNoText);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
					}
					else
					{
						e.Text = String.Empty;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
						Device.BeginInvokeOnMainThread(() => buttonChangeText.Text = btnChangeToHasText);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
					}
				});
			};

			buttonChangeSizeOption.Clicked += (_, __) =>
			{
				editors.ForEach(e =>
				{
					EditorAutoSizeOption option = EditorAutoSizeOption.TextChanges;
					if (e.AutoSize == option)
						option = EditorAutoSizeOption.Disabled;

					e.AutoSize = option;
				});
			};

			StackLayout commands = new StackLayout();
			commands.Children.Add(buttonChangeFont);
			commands.Children.Add(buttonChangeText);
			commands.Children.Add(buttonChangeSizeOption);


			Label sizeOption = new Label();
			sizeOption.SetBinding(Label.TextProperty, new Binding(nameof(Editor.AutoSize), source: editor));

			container.Children.Add(sizeOption);

			var valueConverter = new InvariantConverter();

			editors.ForEach(e =>
			{
				StackLayout commandLayout = new StackLayout() { Orientation = StackOrientation.Horizontal };
				commands.Children.Add(commandLayout);

				Label automationLabelId = new Label();
				automationLabelId.SetBinding(Label.TextProperty, new Binding(nameof(Editor.AutomationId), source: e));

				Label width = new Label();
				width.SetBinding(Label.TextProperty, new Binding(nameof(Editor.Width), source: e, converter: valueConverter));
				width.AutomationId = e.AutomationId + "_width";

				Label height = new Label();
				height.SetBinding(Label.TextProperty, new Binding(nameof(Editor.Height), source: e, converter: valueConverter));
				height.AutomationId = e.AutomationId + "_height";

				commandLayout.Children.Add(automationLabelId);
				commandLayout.Children.Add(width);
				commandLayout.Children.Add(height);
			});

			StackLayout content = new StackLayout();

			content.Children.Add(new ScrollView() { Content = container });
			content.Children.Add(commands);

			Content = content;
		}

		class InvariantConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return ((double)value).ToString(CultureInfo.InvariantCulture);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
	}
}
