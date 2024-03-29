﻿using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2270, "NSInternalCOnsistencyException when bound to ObservableCollection", PlatformAffected.iOS)]
	public class Issue2270 : ContentPage
	{
		public Issue2270()
		{
			BindingContext = new TestListViewModel();

			Padding = new Thickness(0, 40, 0, 0);

			var btn = new Button
			{
				Text = "Load Data",
				BorderWidth = 1,
				BorderColor = Colors.Gray,
			};
			btn.SetBinding(Button.CommandProperty, "LoadDataCommand");

			var lv = new ListView();// { IsGroupingEnabled = true };

			var dt = new DataTemplate(typeof(TextCell));
			dt.SetBinding(TextCell.TextProperty, "Name");

			lv.ItemTemplate = dt;
			lv.SetBinding(ListView.ItemsSourceProperty, "Rows");

			Content = new StackLayout
			{
				Children = {
					btn,
					lv
				}
			};

		}

		public class TestListViewModel
		{
			//public ObservableCollection<ObservableCollection<Row>> Rows {
			public ObservableCollection<Row> Rows
			{
				get;
				set;
			}

			public TestListViewModel()
			{
				//Rows = new ObservableCollection<ObservableCollection<Row>>();
				Rows = new ObservableCollection<Row>();
			}

			Command _command;

			public Command LoadDataCommand
			{
				get
				{
					return _command ?? (_command = new Command(LoadData));
				}
			}

			void LoadData()
			{
				Rows.Clear();

				foreach (var row in new[] { new Row { Name = "one" }, new Row { Name = "Two" } })
				{
					Rows.Add(row);
				}
			}

		}

		public class Row
		{
			public string Name { get; set; }
		}
	}
}
