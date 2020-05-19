using System;

using Xamarin.Forms.CustomAttributes;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 3292, "TableSection.Title property binding fails in XAML")]
	public class Issue3292 : TestContentPage 
	{
		protected override void Init ()
		{
			var vm = new SomePageViewModel ();
			BindingContext = vm;

			var tableview = new TableView ();
			var section = new TableSection ();
			section.SetBinding (TableSectionBase.TitleProperty, new Binding ("SectionTitle"));
			var root = new TableRoot ();
			root.Add (section);
			tableview.Root = root;

			Content = tableview;

			vm.Init ();
		}

		[Preserve (AllMembers = true)]
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
						Device.BeginInvokeOnMainThread(() => {
							SectionTitle = "Hello World Changed";
						});
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
				if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

#if UITEST
		[Test]
		public void Issue3292Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Hello World Changed"));
		}
#endif
	}
}
