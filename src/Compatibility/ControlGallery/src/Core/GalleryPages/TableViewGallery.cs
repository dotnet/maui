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

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class TableViewGallery : ContentPage
	{
		public TableViewGallery()
		{

			var section = new TableSection("Section One") {
				new ViewCell { View = new Label { Text = "View Cell 1" } },
				new ViewCell { View = new Label { Text = "View Cell 2" } }
			};

			var root = new TableRoot("Table") {
				section
			};

			var tableLayout = new TableView
			{
				Root = root,
				RowHeight = 100
			};

			Content = tableLayout;
		}
	}
}
