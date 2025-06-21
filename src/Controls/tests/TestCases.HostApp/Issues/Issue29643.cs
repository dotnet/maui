using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29643, "iOS soft keyboard causes CollectionView to jump", PlatformAffected.iOS)]
public partial class Issue29643 : ContentPage
{
	public Issue29643()
	{
		var grid = new Grid();
		var itemTemplate = new DataTemplate(() =>
		{
			var border = new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = 16 },
				Margin = new Thickness(16),
				Background = Brush.White
			};
			var vsl = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(0, 16) };
			border.Content = vsl;

			vsl.SetBinding(BindableLayout.ItemsSourceProperty, new Binding("Inputs"));
			BindableLayout.SetItemTemplateSelector(vsl, new MyDataTemplateSelector());

			return border;
		});
		var cv = new CollectionView {
			AutomationId = "CV",
			ItemsSource = new List<Items>
			{
				new(15),
				new(3),
				new(15),
				new(3),
				new(15),
				new(3),
				new(15),
				new(3),
			},
			ItemTemplate = itemTemplate
		};
		grid.Add(cv);
		Content = grid;
		Background = Brush.LightSeaGreen;
	}

	private partial class MyDataTemplateSelector : DataTemplateSelector
	{
		private readonly DataTemplate _entryTemplate = new(() =>
		{
			var entry = new Entry
			{
				FontSize = 18,
				HeightRequest = 40,
				BackgroundColor = Colors.LightGoldenrodYellow,
				Margin = new Thickness(16, 0)
			};

			entry.SetBinding(Entry.PlaceholderProperty, new Binding("Label"));
			entry.SetBinding(Element.AutomationIdProperty, new Binding("Label"));
			entry.SetBinding(Entry.TextProperty, new Binding("Content", BindingMode.TwoWay));
			return entry;
		});
		
		private readonly DataTemplate _editorTemplate = new(() =>
		{
			var entry = new AutoSizedEditor
			{
				FontSize = 18,
				BackgroundColor = Colors.LightGoldenrodYellow,
				Margin = new Thickness(16, 0)
			};

			entry.SetBinding(Editor.PlaceholderProperty, new Binding("Label"));
			entry.SetBinding(Element.AutomationIdProperty, new Binding("Label"));
			entry.SetBinding(Editor.TextProperty, new Binding("Content", BindingMode.TwoWay));

			return entry;
		});

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var input = (Input)item;

			if (input.Number % 2 == 1)
			{
				return _editorTemplate;
			}
			
			return _entryTemplate;
		}

		[GeneratedRegex(@"(\d+)$")]
		private static partial Regex GetNumberRegex();
	}

	private class AutoSizedEditor : Editor
	{
		public AutoSizedEditor()
		{
			AutoSize = EditorAutoSizeOption.TextChanges;
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
#if IOS
			if (Handler?.PlatformView is UIKit.UITextView textView)
			{
				textView.ScrollEnabled = false;
			}
#endif
		}
	}

	private record Input(
		string Label,
		int Number,
		string Content
	);

	private class Items
	{
		private static int IdCounter;

		public List<Input> Inputs { get; }

		public Items(int count)
		{
			Inputs = [];
			for (int i = 0; i < count; i++)
			{
				Inputs.Add(new Input($"Input{++IdCounter}", IdCounter, string.Empty));
			}
		}
	}
}