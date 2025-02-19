using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6644, "Android - CollectionView won't animate item remove if EmptyViewTemplate is provided ", PlatformAffected.Android)]
	public partial class Issue6644 : ContentPage
	{
#if APP
		public Issue6644()
		{
			InitializeComponent();
			cv.ItemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10).ToList());
		}

		void Button1_Clicked(object sender, EventArgs e)
		{
			var src = ((ObservableCollection<int>)cv.ItemsSource);
			if (src.Any())
				src.RemoveAt(0);
		}
#endif
	}
}