using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	public class Issue2470ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	[Preserve (AllMembers = true)]
	public class EntryViewModel : ViewModelBase
	{
		string _name;
		public string Name
		{
			get { return _name; }
			set { _name = value; OnPropertyChanged (); }
		}

		bool _selected;
		public bool Selected
		{
			get { return _selected; }
			set { _selected = value; OnPropertyChanged (); }
		}
	}

	[Preserve (AllMembers = true)]
	public class Issue2470MainViewModel : Issue2470ViewModelBase
	{
		public ObservableCollection<EntryViewModel> Entries { get; private set; }

		double _desiredCount;
		public double DesiredCount
		{
			get { return _desiredCount; }
			set
			{
				_desiredCount = value;
				OnPropertyChanged ();
				GenerateEntries ();
			}
		}

		bool _twoOrFive;
		public bool TwoOrFive
		{
			get { return _twoOrFive; }
			set
			{
				_twoOrFive = value;
				OnPropertyChanged ();
				DesiredCount = _twoOrFive ? 5 : 2;
			}
		}

		public Issue2470MainViewModel ()
		{
			Entries = new ObservableCollection<EntryViewModel> ();
			TwoOrFive = false; // prime
		}

		void GenerateEntries ()
		{
			Entries.Clear ();
			for (var i = 0; i < DesiredCount; i++) {
				Entries.Add (new EntryViewModel { Name = "Entry " + i + " of " + DesiredCount });
			}
		}
	}

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2470, "ObservableCollection changes do not update ListView", PlatformAffected.Android)]
	public partial class Issue2470 : TestTabbedPage
	{
		protected override void Init ()
		{
			var mainViewModel = new Issue2470MainViewModel ();
			BindingContext = mainViewModel;
		}

#if APP
		[Preserve (AllMembers = true)]
		public Issue2470 ()
		{
			InitializeComponent ();
		}
#endif

#if UITEST
		[Test]
		public void OnservableCollectionChangeListView ()
		{
			// Tab 1
			RunningApp.Tap (q => q.Marked ("Switch"));
			RunningApp.Screenshot ("Switch On");
			RunningApp.Tap (q => q.Marked ("Results"));

			// Tab 2
			RunningApp.WaitForElement (q => q.Marked ("Entry 0 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 1 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 2 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 3 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 4 of 5"));
			RunningApp.Screenshot ("Should be 5 elements");
			RunningApp.Tap (q => q.Marked ("Generate"));
			
			// Tab 1
			RunningApp.Tap (q => q.Marked ("Switch"));	
			RunningApp.Screenshot ("Switch Off");
			RunningApp.Tap (q => q.Marked ("Results"));

			// Tab 2
			RunningApp.WaitForElement (q => q.Marked ("Entry 0 of 2"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 1 of 2"));
			RunningApp.Screenshot ("Should be 2 elements");
			
			// Tab 1
			RunningApp.Tap (q => q.Marked ("Generate"));
			RunningApp.Tap (q => q.Marked ("Switch"));
			RunningApp.Screenshot ("Switch On");
			RunningApp.Tap (q => q.Marked ("Results"));

			// Tab 2
			RunningApp.WaitForElement (q => q.Marked ("Entry 0 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 1 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 2 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 3 of 5"));
			RunningApp.WaitForElement (q => q.Marked ("Entry 4 of 5"));
			RunningApp.Screenshot ("Should be 5 elements");
		}
#endif
	}
}
