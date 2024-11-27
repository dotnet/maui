using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 55365, "~VisualElement crashes with System.Runtime.InteropServices.COMException", PlatformAffected.UWP)]
	public class Bugzilla55365 : TestContentPage
	{
		readonly StackLayout _itemsPanel = new StackLayout();
		readonly DataTemplate _itemTemplate = new DataTemplate(CreateBoxView);
		readonly StackLayout _layout = new StackLayout();

		protected override void Init()
		{
			var viewModel = new ObservableCollection<_55365Item>
			{
				new _55365Item { Subject = 65 }
			};

			viewModel.CollectionChanged += OnCollectionChanged;

			_itemsPanel.BindingContext = viewModel;

			foreach (_55365Item item in viewModel)
			{
				_itemTemplate.SetValue(BindingContextProperty, item);
				var view = (View)_itemTemplate.CreateContent();
				_itemsPanel.Children.Add(view);
			}

			var clearButton = new Button { AutomationId = "Clear", Text = "Clear", Command = new Command(o => viewModel.Clear()) };
			_layout.Children.Add(clearButton);

			var collectButton = new Button
			{
				AutomationId = "Garbage",
				Text = "Garbage",
				Command = new Command(o =>
				{
					GarbageCollectionHelper.Collect();
					_layout.Children.Add(new Label { Text = "Success" });
				})
			};
			_layout.Children.Add(collectButton);
			_layout.Children.Add(_itemsPanel);

			Content = _layout;
		}

		static object CreateBoxView()
		{
			var boxView1 = new BoxView { HeightRequest = 100, Color = new Color(0.55f, 0.23f, 0.147f) };
			var setter1 = new Setter { Property = BoxView.ColorProperty, Value = "#FF2879DD" };
			var trigger1 = new DataTrigger(typeof(BoxView)) { Binding = new Binding("Subject"), Value = 65 };
			trigger1.Setters.Add(setter1);
			boxView1.Triggers.Add(trigger1);
			return boxView1;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				// reset the list
				_itemsPanel.Children.Clear();
			}
		}


		public class _55365Item
		{
			public int Subject { get; set; }
		}
	}
}