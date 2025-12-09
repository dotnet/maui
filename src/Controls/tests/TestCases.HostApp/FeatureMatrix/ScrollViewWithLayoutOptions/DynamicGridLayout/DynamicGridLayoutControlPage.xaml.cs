namespace Maui.Controls.Sample;

public partial class DynamicGridLayoutControlPage : ContentPage
{
	private LayoutViewModel _viewModel;
	private bool _addByRow = true;

	public DynamicGridLayoutControlPage()
	{
		InitializeComponent();
		_viewModel = new LayoutViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		_viewModel.RowCount = 2;
		_viewModel.ColumnCount = 2;
		_viewModel.LabelCount = 0;

		BuildDynamicGrid();

		for (int i = 0; i < 3; i++)
		{
			_viewModel.LabelCount++;
			if (_addByRow)
				AddByRow();
			else
				AddByColumn();
		}
	}

	private void BuildDynamicGrid()
	{
		DynamicGrid.Children.Clear();
		DynamicGrid.RowDefinitions.Clear();
		DynamicGrid.ColumnDefinitions.Clear();

		if (_addByRow)
		{
			for (int i = 0; i < _viewModel.ColumnCount; i++)
				DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

			for (int i = 0; i < _viewModel.RowCount - 1; i++)
				DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			DynamicGrid.BackgroundColor = Colors.Pink;
			DynamicGrid.HorizontalOptions = LayoutOptions.Fill;
			DynamicGrid.VerticalOptions = LayoutOptions.Start;
			MyScrollView.HorizontalOptions = LayoutOptions.Fill;
			MyScrollView.VerticalOptions = LayoutOptions.Start;
			MyScrollView.Orientation = ScrollOrientation.Vertical;
		}
		else
		{
			for (int i = 0; i < _viewModel.RowCount; i++)
				DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			for (int i = 0; i < _viewModel.ColumnCount - 1; i++)
				DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

			DynamicGrid.BackgroundColor = Colors.Yellow;
			DynamicGrid.HorizontalOptions = LayoutOptions.Start;
			DynamicGrid.VerticalOptions = LayoutOptions.Fill;
			MyScrollView.HorizontalOptions = LayoutOptions.Start;
			MyScrollView.VerticalOptions = LayoutOptions.Fill;
			MyScrollView.Orientation = ScrollOrientation.Horizontal;
		}
	}

	private Label CreateLabel(int index)
	{
		return new Label
		{
			Text = $"Label {index}",
			FontSize = 18,
			Padding = new Thickness(10)
		};
	}

	private void OnAddChildClicked(object sender, EventArgs e)
	{
		if (DynamicGrid == null)
			return;

		_viewModel.LabelCount++;

		if (_addByRow)
			AddByRow();
		else
			AddByColumn();
	}

	private void AddByRow()
	{
		int rows = DynamicGrid.RowDefinitions.Count;
		int columns = _viewModel.ColumnCount;

		int totalCells = rows * columns;
		if (_viewModel.LabelCount > totalCells)
		{
			DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			_viewModel.RowCount++;
			rows++;
		}

		int newIndex = _viewModel.LabelCount - 1;
		int row = newIndex / columns;
		int column = newIndex % columns;

		var lbl = CreateLabel(_viewModel.LabelCount);
		Grid.SetRow(lbl, row);
		Grid.SetColumn(lbl, column);
		DynamicGrid.Children.Add(lbl);
	}

	private void AddByColumn()
	{
		int columns = DynamicGrid.ColumnDefinitions.Count;
		int rows = _viewModel.RowCount;

		int totalCells = rows * columns;
		if (_viewModel.LabelCount > totalCells)
		{
			DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			_viewModel.ColumnCount++;
			columns++;
		}

		int newIndex = _viewModel.LabelCount - 1;
		int column = newIndex / rows;
		int row = newIndex % rows;

		var lbl = CreateLabel(_viewModel.LabelCount);
		Grid.SetRow(lbl, row);
		Grid.SetColumn(lbl, column);
		DynamicGrid.Children.Add(lbl);
	}

	private void OnRemoveChildClicked(object sender, EventArgs e)
	{
		if (DynamicGrid == null || DynamicGrid.Children.Count == 0)
			return;

		DynamicGrid.Children.RemoveAt(DynamicGrid.Children.Count - 1);
		_viewModel.LabelCount = Math.Max(0, _viewModel.LabelCount - 1);
	}

	private void OnAddModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		if (AddByRowButton.IsChecked)
			_addByRow = true;
		else
			_addByRow = false;

		_viewModel.LabelCount = 0;
		_viewModel.RowCount = 2;
		_viewModel.ColumnCount = 2;

		BuildDynamicGrid();

		for (int i = 0; i < 3; i++)
			OnAddChildClicked(this, EventArgs.Empty);
	}
}