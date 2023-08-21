//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

#if APP
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5268, "ListView with PullToRefresh enabled gestures conflict", PlatformAffected.Android)]
	public partial class Issue5268 : ContentPage
	{
		[Preserve(AllMembers = true)]
		public class SrcItem
		{
			public string Val { get; set; }
		}

		string GenerateLongString() => string.Join(" \n", Enumerable.Range(0, 50).Select(i => $"{Sources.Count} item"));

		public ObservableCollection<SrcItem> Sources { get; }
		public ICommand Command { get; }

		public Issue5268()
		{
			InitializeComponent();
			Sources = new ObservableCollection<SrcItem>();
			Command = new Command(AddData);
			Sources.Add(new SrcItem { Val = GenerateLongString() });
			MyListView.BindingContext = this;
		}

		void AddData()
		{
			IsBusy = true;
			Sources.Add(new SrcItem { Val = GenerateLongString() });
			IsBusy = false;
		}
	}
}
#endif