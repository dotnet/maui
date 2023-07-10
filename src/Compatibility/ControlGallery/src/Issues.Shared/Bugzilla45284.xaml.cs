using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45284, "[iOS10] Extra tab icons display in iOS when binding Title on TabbedPage", PlatformAffected.iOS)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
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