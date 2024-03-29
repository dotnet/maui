﻿using System.Collections;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8508, "[Bug] UWP CollectionView.Scrolled not raised", PlatformAffected.UWP)]
	public partial class Issue8508 : TestContentPage
	{
		public Issue8508()
		{
#if APP
			Title = "Issue 8508";
			InitializeComponent();
			collectionView.ItemsSource = Enumerable.Range(1, 100);
#endif
		}

		protected override void Init()
		{

		}

		void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			DisplayAlert("Scrolled", $"VerticalOffset: {e.VerticalOffset}", "Ok");

			Debug.WriteLine("HorizontalDelta: " + e.HorizontalDelta);
			Debug.WriteLine("VerticalDelta: " + e.VerticalDelta);
			Debug.WriteLine("HorizontalOffset: " + e.HorizontalOffset);
			Debug.WriteLine("VerticalOffset: " + e.VerticalOffset);
			Debug.WriteLine("FirstVisibleItemIndex: " + e.FirstVisibleItemIndex);
			Debug.WriteLine("CenterItemIndex: " + e.CenterItemIndex);
			Debug.WriteLine("LastVisibleItemIndex: " + e.LastVisibleItemIndex);
		}
	}
}