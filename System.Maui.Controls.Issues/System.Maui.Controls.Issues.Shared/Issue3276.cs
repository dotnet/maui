using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 3276, "Crashing Unknown cell parent type on ContextAction Bindings")]
	public class Issue3276 : TestTabbedPage
	{
		protected override void Init ()
		{
			var listview = new ListView ();
			listview.ItemTemplate = new DataTemplate (typeof(CaCell));

			listview.SetBinding (ListView.ItemsSourceProperty, new Binding ("SearchResults"));

			var page = new ContentPage {Title = "First", Content = listview, BindingContext = new VM () };

			page.Appearing += (object sender, EventArgs e) => (page.BindingContext as VM).Load ();

			Children.Add (page);
			Children.Add (new ContentPage { Title = "Second" });
		}

		[Preserve (AllMembers = true)]
		public class VM : ViewModel
		{
			public void Load ()
			{
				var list = new List<string> ();
				for (int i = 0; i < 20; i++) {
					list.Add ("second " + i.ToString ());
				}
				SearchResults = new ObservableCollection<string> (list);
			}

			ObservableCollection<string> _list = null;

			public ObservableCollection<string> SearchResults {
				get { return _list; }

				set {
					_list = value;
					OnPropertyChanged ();
				}
			}

		}

		[Preserve (AllMembers = true)]
		public class CaCell : ViewCell
		{
			public CaCell ()
			{
				var label = new Label ();
				label.SetBinding (Label.TextProperty, new Binding ("."));
				var menu = new MenuItem { Text = "Delete", IsDestructive = true };
				menu.SetBinding (MenuItem.CommandParameterProperty, new Binding ("."));
				var menu1 = new MenuItem { Text = "Settings"  };
				menu1.SetBinding (MenuItem.CommandParameterProperty, new Binding ("."));
				ContextActions.Add (menu);
				ContextActions.Add (menu1);

				var stack = new StackLayout ();
				stack.Children.Add (label);
				View = stack;
			}
		}


		#if UITEST
		[Test]
		public void Issue3276Test ()
		{
			RunningApp.Tap (q => q.Marked ("Second"));
			RunningApp.Tap (q => q.Marked ("First"));
			RunningApp.WaitForElement (q => q.Marked ("second 1"));
		}
#endif
	}
}
