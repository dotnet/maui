using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 3788, "[UWP] ListView with observable collection always seems to refresh the entire list",
	PlatformAffected.UWP)]
public class Issue3788 : TestContentPage
{
	const string _replaceMe = "Replace Me";
	const string _last = "Last";
	const string _buttonText = "Scroll down and click me";
	protected override void Init()
	{
		ChangingList data =
			new ChangingList(Enumerable.Range(0, 1000).Select(_ => new TestModel()));

		data.Add(new TestModel() { Text = _replaceMe });
		ListView view = new ListView();

		view.ItemTemplate = new DataTemplate(() =>
		{
			ViewCell cell = new ViewCell();
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			cell.View = label;
			return cell;
		});

		view.ItemsSource = data;
#pragma warning disable CS0618 // Type or member is obsolete
		view.VerticalOptions = LayoutOptions.StartAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
		view.ScrollTo(data.Last(), ScrollToPosition.End, false);
		Content = new StackLayout()
		{
			view,
			new Button()
			{
				Text = _buttonText,
				Command = new Command(() =>
				{
					data.Test();
				})
			}
		};
	}
	public class ChangingList : List<TestModel>, INotifyCollectionChanged
	{
		public ChangingList(IEnumerable<TestModel> collection) : base(collection)
		{
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void Test()
		{
			var oldItem = this[this.Count - 1];
			this[this.Count - 1] = new TestModel() { Text = _last };
			CollectionChanged?.Invoke(this,
				new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace,
						this[this.Count - 1], oldItem, this.Count - 1
					));
		}
	}
	public class TestModel
	{
		public string Text { get; set; } = Guid.NewGuid().ToString();
		public override string ToString()
		{
			return Text;
		}
	}
}
