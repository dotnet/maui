// Thanks to GitHub user [@Matmork](https://github.com/Matmork) for this reproducible test case.
// https://github.com/xamarin/Xamarin.Forms/issues/9711#issuecomment-602520024
// Now living on in .NET MAUI! Love our community! <3

using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9711, "[Bug] iOS Failed to marshal the Objective-C object HeaderWrapperView", PlatformAffected.iOS)]
public partial class Issue9711 : TestContentPage
{
	protected override void Init()
	{
		InitializeComponent();

		List<ListGroup<string>> groups = new List<ListGroup<string>>();
		for (int i = 0; i < 105; i++)
		{
			var group = new ListGroup<string> { Title = $"Group{i}" };
			for (int j = 0; j < 5; j++)
			{
				group.Add($"Group {i} Item {j}");
			}

			groups.Add(group);
		}

		TestListView.AutomationId = "9711TestListView";
		TestListView.ItemsSource = groups;
	}

	private void ViewCell_OnBindingContextChanged(object sender, EventArgs e)
	{
		if (sender is ViewCell cell && cell.BindingContext is ListGroup<string> list)
		{
			list.Cell = cell;
		}
	}

	private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
	{
		if (sender is ContentView cnt && cnt.BindingContext is ListGroup<string> list)
		{
			for (int i = 0; i <= 50; i++)
			{
				await Task.Delay(25);
				list.IsExpanded = !list.IsExpanded;
			}
		}
	}
}

public sealed class ListGroup<T> : List<T>, INotifyPropertyChanged, INotifyCollectionChanged
{
	public string Title { get; set; }
	public string AutomationId => Title;
	private bool _isExpanded = true;

	public bool IsExpanded
	{
		get => _isExpanded;
		set
		{
			if (_isExpanded == value)
				return;

			Cell?.Height = value ? 75 : 40;

			_isExpanded = value;
			OnPropertyChanged();
			OnCollectionChanged();
		}
	}

	public ViewCell Cell { get; set; }
	public event NotifyCollectionChangedEventHandler CollectionChanged;
	public event PropertyChangedEventHandler PropertyChanged;

	private void OnCollectionChanged()
	{
		CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	private void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
