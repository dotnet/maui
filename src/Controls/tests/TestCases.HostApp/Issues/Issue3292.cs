using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Github, 3292, "TableSection.Title property binding fails in XAML")]
public class Issue3292 : TestContentPage
{
	protected override void Init()
	{
		var vm = new SomePageViewModel();
		BindingContext = vm;

		var tableview = new TableView();
		var section = new TableSection();
		section.SetBinding(TableSectionBase.TitleProperty, new Binding("SectionTitle"));
		var root = new TableRoot();
		root.Add(section);
		tableview.Root = root;

		Content = tableview;

		vm.Init();
	}

	[Preserve(AllMembers = true)]
	public class SomePageViewModel : INotifyPropertyChanged
	{
		string _sectionTitle;

		public SomePageViewModel()
		{
			SectionTitle = "Hello World";
		}

		public void Init()
		{
			Task.Delay(1000).ContinueWith(t =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
					Device.BeginInvokeOnMainThread(() =>
					{
						SectionTitle = "Hello World Changed";
					});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
				});
		}

		public string SectionTitle
		{
			get { return _sectionTitle; }
			set
			{
				_sectionTitle = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
