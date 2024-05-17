using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2282, "ListView ItemTapped issue on Windows phone", PlatformAffected.WinPhone)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue2282 : ContentPage
	{
		public Issue2282()
		{
			Items = new ObservableCollection<string>();
			InitializeComponent();

			BindingContext = Items;
			MyListView.ItemTapped += (sender, e) =>
			{
				LogLabel.Text = string.Format("{0} - Item {1} Tapped!", _counter++, (string)e.Item);
			};
		}

		public ObservableCollection<string> Items { get; set; }

		int _counter = 0;

		protected override void OnAppearing()
		{
			Items.Add("First");
			Items.Add("Second");
			Items.Add("Third");
			base.OnAppearing();
		}
	}
#endif
}
