using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Shapes = Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Pages
{
	public class LayoutUpdatesPage : Base.BasePage
	{
		public LayoutUpdatesPage()
		{
			var root = new GridLayout() { Margin = 40, BackgroundColor = Colors.Beige };

			root.RowDefinitions = new RowDefinitionCollection()
			{
				new RowDefinition(){ Height = GridLength.Auto },
				new RowDefinition(){ Height = GridLength.Auto },
				new RowDefinition(){ Height = GridLength.Auto },
				new RowDefinition(){ Height = GridLength.Auto }
			};

			root.ColumnDefinitions = new ColumnDefinitionCollection()
			{
				new ColumnDefinition(){ Width = GridLength.Star },
				new ColumnDefinition(){ Width = GridLength.Star },
			};

			var addButton = new Button { Text = "Add" };
			root.Add(addButton);

			var removeButton = new Button { Text = "Remove" };
			root.Add(removeButton);
			root.SetColumn(removeButton, 1);

			var insertButton = new Button { Text = "Insert" };
			root.Add(insertButton);
			root.SetRow(insertButton, 1);

			var clearButton = new Button { Text = "Clear" };
			root.Add(clearButton);
			root.SetColumn(clearButton, 1);
			root.SetRow(clearButton, 1);

			var updateButton = new Button { Text = "Update" };
			root.Add(updateButton);
			root.SetRow(updateButton, 2);

			var stack = new VerticalStackLayout();

			double shapeWidth = 200;
			double shapeHeight = 80;
			double overlap = -20;
			double leftMarginIncrement = 20;

			var r1 = new Shapes.Rectangle() { WidthRequest = shapeWidth, HeightRequest = shapeHeight, Fill = new SolidColorBrush(NextColor()), Margin = new Thickness(0, 0, 0, 0) };
			var r2 = new Shapes.Rectangle() { WidthRequest = shapeWidth, HeightRequest = shapeHeight, Fill = new SolidColorBrush(NextColor()), Margin = new Thickness(leftMarginIncrement, overlap, 0, 0) };

			stack.Add(r1);
			stack.Add(r2);

			root.Add(stack);
			root.SetRow(stack, 3);
			root.SetColumnSpan(stack, 2);

			addButton.Clicked += (sender, args) =>
			{
				var left = leftMarginIncrement * stack.Count;
				var rect = new Shapes.Rectangle() { WidthRequest = shapeWidth, HeightRequest = shapeHeight, Fill = new SolidColorBrush(NextColor()), Margin = new Thickness(left, overlap, 0, 0) };
				stack.Add(rect);
			};

			insertButton.Clicked += (sender, args) =>
			{

				if (stack.Count < 2)
				{
					return;
				}

				var left = leftMarginIncrement * stack.Count;

				var rect = new Shapes.Rectangle() { WidthRequest = shapeWidth, HeightRequest = shapeHeight, Fill = new SolidColorBrush(NextColor()), Margin = new Thickness(left, overlap, 0, 0) };
				stack.Insert(1, rect);
			};

			clearButton.Clicked += (sender, args) =>
			{
				stack.Clear();
			};

			removeButton.Clicked += (sender, args) =>
			{
				if (stack.Count > 0)
				{
					stack.RemoveAt(stack.Count - 1);
				}
			};

			updateButton.Clicked += (sender, args) =>
			{
				if (stack.Count > 0)
				{
					var left = leftMarginIncrement * stack.Count;
					var rect = new Shapes.Rectangle() { WidthRequest = shapeWidth, HeightRequest = shapeHeight, Fill = new SolidColorBrush(NextColor()), Margin = new Thickness(left, overlap, 0, 0) };
					stack[0] = rect;
				}
			};

			Content = root;
		}

		int _colorIndex;

		Color[] _colors = new Color[]
		{
			Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.Orange, Colors.Purple
		};

		Color NextColor()
		{
			var color = _colors[_colorIndex];

			_colorIndex += 1;
			if (_colorIndex >= _colors.Length)
			{
				_colorIndex = 0;
			}

			return color;
		}
	}
}
