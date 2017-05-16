using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;

#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55365, "~VisualElement crashes with System.Runtime.InteropServices.COMException", PlatformAffected.UWP)]
	public class Bugzilla55365 : TestContentPage
	{
		readonly StackLayout _itemsPanel = new StackLayout();
		readonly DataTemplate _itemTemplate = new DataTemplate(CreateBoxView);
		readonly StackLayout _layout = new StackLayout();

#if UITEST
		[Test]
		public void ForcingGCDoesNotCrash()
		{
			RunningApp.WaitForElement("Clear");
			RunningApp.Tap("Clear");
			RunningApp.Tap("Garbage");
			RunningApp.WaitForElement("Success");
		}
#endif

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

			var clearButton = new Button { Text = "Clear", Command = new Command(o => viewModel.Clear()) };
			_layout.Children.Add(clearButton);

			var collectButton = new Button { Text = "Garbage", Command = new Command(o =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				_layout.Children.Add(new Label {Text = "Success"});
			}) };
			_layout.Children.Add(collectButton);
			_layout.Children.Add(_itemsPanel);

			Content = _layout;
		}

		static object CreateBoxView()
		{
			var boxView1 = new BoxView { HeightRequest = 100, Color = new Color(0.55, 0.23, 0.147) };
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

		[Preserve(AllMembers = true)]
		public class _55365Item
		{
			public int Subject { get; set; }
		}
	}
}