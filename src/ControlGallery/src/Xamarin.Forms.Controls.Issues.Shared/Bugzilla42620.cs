using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42620, " Grid.Children.AddHorizontal does not span all rows", PlatformAffected.Default)]
	public class Bugzilla42620 : TestContentPage
	{
		public static IEnumerator<Color> Colors = ColorGenerator().GetEnumerator();
		public static Color GetColor()
		{
			Colors.MoveNext();
			var color = Colors.Current;
			return color;
		}

		public static Tuple<int, int, int> Mix = new Tuple<int, int, int>(255, 255, 100);
		public static IEnumerable<Color> ColorGenerator()
		{
			while (true)
			{
				var rand = new Random();
				var r = rand.Next(256);
				var g = rand.Next(256);
				var b = rand.Next(256);

				if (Mix != null)
				{
					r = (r + Mix.Item1) / 2;
					g = (g + Mix.Item2) / 2;
					b = (b + Mix.Item3) / 2;
				}

				yield return Color.FromRgb(r, g, b);
			}
		}

		public View CreateBox()
		{
			var layout = new AbsoluteLayout();

			var color = GetColor();

			var box = new BoxView()
			{
				BackgroundColor = color
			};
			layout.Children.Add(box, Rectangle.FromLTRB(0, 0, 1, 1), AbsoluteLayoutFlags.All);

			var label = new Label()
			{
				AutomationId = $"{s_id++}",
				TextColor = Color.Black,
				BackgroundColor = color,
				VerticalOptions = LayoutOptions.Center
			};
			layout.Children.Add(label, Rectangle.FromLTRB(0, 0, 1, 1), AbsoluteLayoutFlags.All);
			var tgr = new TapGestureRecognizer();
			tgr.Tapped += (x, o) =>
			{
				Grid.SetColumnSpan(layout, Grid.GetColumnSpan(layout) + 1);
			};
			label.GestureRecognizers.Add(tgr);

			return layout;
		}
		private static int s_id = 0;
		protected override void Init()
		{
			var stack = new StackLayout();
			var columnDefinitionsLabel = new Label();
			var rowDefinitionsLabel = new Label();
			var operationLabel = new Label();
			var batchLabel = new Label();

			var batchEntry = new Entry() { AutomationId = "batch" };

			var grid = new Grid();
			stack.Children.Add(grid);

			Action update = () =>
			{
				var col = "";
				foreach (var def in grid.ColumnDefinitions)
					col += $"{def.Width} ";
				columnDefinitionsLabel.Text = $"Col: {grid.ColumnDefinitions.Count()}={col}";

				var row = "";
				foreach (var def in grid.RowDefinitions)
					row += $"{def.Height} ";
				rowDefinitionsLabel.Text = $"Row: {grid.RowDefinitions.Count()}={row}";

				operationLabel.Text = $"Id: {s_id}";

				foreach (AbsoluteLayout layout in grid.Children)
				{
					var label = (Label)layout.Children[1];
					label.Text = $"{label.AutomationId}: " +
						$"{Grid.GetColumn(layout)}x{Grid.GetRow(layout)} " +
						$"{Grid.GetColumnSpan(layout)}x{Grid.GetRowSpan(layout)}";
				}
			};
			grid.LayoutChanged += (o, x) => update();

			var dashboard = new StackLayout();
			stack.Children.Add(dashboard);
			dashboard.Children.Add(columnDefinitionsLabel);
			dashboard.Children.Add(rowDefinitionsLabel);
			dashboard.Children.Add(operationLabel);
			dashboard.Children.Add(batchLabel);

			var buttons = new Grid();
			stack.Children.Add(buttons);

			var addRow = new Button() { Text = "R" };
			addRow.Clicked += (o, e) =>
			{
				grid.RowDefinitions.Add(new RowDefinition());
				update();
			};
			buttons.Children.AddHorizontal(addRow);

			var addColumn = new Button() { Text = "C" };
			addColumn.Clicked += (o, e) =>
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				update();
			};
			buttons.Children.AddHorizontal(addColumn);

			var addHorizontal = new Button() { Text = "H" };
			addHorizontal.Clicked += (o, e) =>
				grid.Children.AddHorizontal(CreateBox());
			buttons.Children.AddHorizontal(addHorizontal);

			var addVertical = new Button() { Text = "V" };
			addVertical.Clicked += (o, e) =>
				grid.Children.AddVertical(CreateBox());
			buttons.Children.AddHorizontal(addVertical);

			var clearButton = new Button() { Text = "*" };
			clearButton.Clicked += (o, e) =>
			{
				grid.Children.Clear();
				grid.ColumnDefinitions.Clear();
				grid.RowDefinitions.Clear();
				update();
			};
			buttons.Children.AddHorizontal(clearButton);

			var command = new Grid();
			stack.Children.Add(command);

			command.Children.AddHorizontal(batchEntry);
			Grid.SetColumnSpan(batchEntry, 3);

			var batchButton = new Button() { Text = "!" };
			batchButton.Clicked += (o, e) =>
			{
				clearButton.SendClicked();
				foreach (var x in batchEntry.Text)
				{
					if (x == 'H')
						addHorizontal.SendClicked();
					if (x == 'V')
						addVertical.SendClicked();
					if (x == 'C')
						addColumn.SendClicked();
					if (x == 'R')
						addRow.SendClicked();
				}
				batchLabel.Text = $"Batch: {batchEntry.Text}";
				update();
				batchEntry.Text = string.Empty;
			};
			command.Children.AddHorizontal(batchButton);

			update();
			this.Content = stack;
		}

#if UITEST
		public static void Swap(ref int lhs, ref int rhs)
		{
			var temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		public static class Ids
		{
			public const string AddRow = "R";
			public const string AddColumn = "C";
			public const string AddVertical = "V";
			public const string AddHorizontal = "H";
			public const string Clear = "*";
			public const string Batch = "!";
		}

		int _id = 0;
		int _rowDef = 0;
		int _colDef = 0;
		int _totalWidth = 0;
		int _totalHeight = 0;

		Queue<string> _buttons = new Queue<string>();
		Queue<string> _result = new Queue<string>();

		public void Tap(string button)
		{
			_buttons.Enqueue(button);
		}
		public void WaitForElement(string query)
		{
			_result.Enqueue(query);
		}
		public void Execute()
		{
			RunningApp.EnterText("batch", string.Join("", _buttons));
			RunningApp.DismissKeyboard();
			RunningApp.Tap(Ids.Batch);

			foreach (var result in _result)
				RunningApp.WaitForElement(q => q.Marked($"{result}"));

			_buttons.Clear();
			_result.Clear();
		}

		public void AddHorizontal()
		{
			// new block gets new id
			var id = _id++;

			// adding column only increases height if no rows exist
			if (_totalHeight == 0)
				_totalHeight = 1;

			// adding column always increased width by 1
			_totalWidth++;

			// column spans rows 0 to the last row
			var row = 0;
			var height = _totalHeight;

			// column is always added at the end with a width of 1
			var column = _totalWidth - 1;
			var width = 1;

			Tap(Ids.AddHorizontal);
			WaitForElement($"{id}: {column}x{row} {width}x{height}");
		}

		public void AddVertical()
		{
			// new block gets new id
			var id = _id++;

			// adding row only increases width if no columns exist
			if (_totalWidth == 0)
				_totalWidth = 1;

			// adding row always increased height by 1
			_totalHeight++;

			// row spans columns 0 to the last column
			var column = 0;
			var width = _totalWidth;

			// row is always added at the end with a height of 1
			var row = _totalHeight - 1;
			var height = 1;

			Tap(Ids.AddVertical);
			WaitForElement($"{id}: {column}x{row} {width}x{height}");
		}

		public void AddRowDef()
		{
			Tap(Ids.AddRow);
			_rowDef++;
			_totalHeight = Math.Max(_rowDef, _totalHeight);
		}

		public void AddColumnDef()
		{
			Tap(Ids.AddColumn);
			_colDef++;
			_totalWidth = Math.Max(_colDef, _totalWidth);
		}

		[Test]
		public void GridChildrenAddHorizontalDoesNotSpanAllRows()
		{
			Issue42620Test("RCRHVHVHVHVHV");

			Issue42620Test("HHHV");
			Issue42620Test("VVVH");

			Issue42620Test("RV");
			Issue42620Test("RH");
			Issue42620Test("CV");
			Issue42620Test("CH");

			Issue42620Test("RVRRV");
			Issue42620Test("CHCCH");

			Issue42620Test("HHV");
			Issue42620Test("HHH");
			Issue42620Test("HVV");
			Issue42620Test("HVH");
			//Issue42620Test("VHV");
			Issue42620Test("VHH");
			Issue42620Test("VVV");
			Issue42620Test("VVH");
		}
		public void Issue42620Test(string command)
		{
			RunningApp.WaitForElement(q => q.Marked(Ids.Clear));

			_totalWidth = 0;
			_totalHeight = 0;
			_rowDef = 0;
			_colDef = 0;

			foreach (var c in command)
			{
				if (c == 'H')
					AddHorizontal();
				if (c == 'V')
					AddVertical();
				if (c == 'R')
					AddRowDef();
				if (c == 'C')
					AddColumnDef();
			}

			Execute();
		}
#endif
	}
}