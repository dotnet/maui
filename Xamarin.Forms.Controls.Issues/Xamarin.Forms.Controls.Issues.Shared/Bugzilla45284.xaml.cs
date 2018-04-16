using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45284, "[iOS10] Extra tab icons display in iOS when binding Title on TabbedPage", PlatformAffected.iOS)]
	public partial class Bugzilla45284 : TabbedPage
	{
		public Bugzilla45284()
		{
			var model = new Bugzilla45284Model();
			InitializeComponent();
			BindingContext = model;
			model.Change();
		}
	}
	[Preserve(AllMembers = true)]
	public class Bugzilla45284Model : INotifyPropertyChanged
	{
		public List<Bugzilla45284TabModel> Tabs => new List<Bugzilla45284TabModel> { 
			new Bugzilla45284TabModel(),
			new Bugzilla45284TabModel(),
			new Bugzilla45284TabModel(),
			new Bugzilla45284TabModel(),
			new Bugzilla45284TabModel(),
			new Bugzilla45284TabModel(),
			new Bugzilla45284TabModel(),
		};

		public event PropertyChangedEventHandler PropertyChanged;
		public void Change()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tabs)));
		}
	}
	[Preserve(AllMembers = true)]
	public class Bugzilla45284TabModel
	{
		public string Title { get; set; } = "Title";
		public string Icon { get; set; } = "bank.png";
	}
#endif
}