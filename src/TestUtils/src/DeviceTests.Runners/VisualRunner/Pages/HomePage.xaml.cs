using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Utilities;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
	partial class HomePage : ContentPage
	{
		readonly static IValueConverter AssemblyRunStatusConverter = new RunStatusToColorConverter();

		private HomeViewModel viewModel;

		public HomePage()
		{
			InitializeComponent();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (viewModel != null)
			{
				viewModel.ScanComplete -= ScanComplete;
			}

			// Wire up the sections
			viewModel = (HomeViewModel)BindingContext;
			viewModel.ScanComplete += ScanComplete;

			root.Clear();
		}

		private void ScanComplete(object sender, EventArgs e)
		{
			// Xam Forms requires us to redraw the table root to add new content
			var tr = new TableRoot();
			var fs = new TableSection("Test Assemblies");
			var i = 0;

			var margin = new Thickness(0, 0, 5, 0);
			foreach (var ta in viewModel.TestAssemblies)
			{

				var lblAssm = new Label();
				lblAssm.SetBinding(Label.TextProperty, nameof(ta.DisplayName));
				lblAssm.SetBinding(Label.TextColorProperty, nameof(ta.RunStatus), converter: AssemblyRunStatusConverter);


				var lblPassed = new Label();
				lblPassed.SetBinding(Label.TextProperty, nameof(ta.Passed));
				lblPassed.TextColor = Colors.Green;
				lblPassed.Margin = margin;

				var lblFailed = new Label();
				lblFailed.SetBinding(Label.TextProperty, nameof(ta.Failed));
				lblFailed.TextColor = Colors.Red;
				lblFailed.Margin = margin;

				var lblSkipped = new Label();
				lblSkipped.SetBinding(Label.TextProperty, nameof(ta.Skipped));
				lblSkipped.TextColor = RunStatusToColorConverter.SkippedColor;
				lblSkipped.Margin = margin;

				var lblNotRun = new Label();
				lblNotRun.SetBinding(Label.TextProperty, nameof(ta.NotRun));
				lblNotRun.TextColor = Colors.DarkGray;

				var vs = new CommandViewCell()
				{
					BindingContext = ta,
					View = new StackLayout
					{
						Margin = new Thickness(20, 0, 0, 0),
						Children =
						{
							lblAssm,
							new StackLayout
							{
								Orientation = StackOrientation.Horizontal,
								Children =
								{
									new Label{Text = " ✔ ", TextColor = Colors.Green},
									lblPassed,
									new Label{Text=" ⛔ "},
									lblFailed,
									new Label{Text = " ⚠ ", TextColor = RunStatusToColorConverter.SkippedColor},
									lblSkipped,
									new Label{Text=" 🔷 "},
									lblNotRun
								}
							}
						}
					},
					CommandParameter = ta,
					Command = viewModel.NavigateToTestAssemblyCommand
				};

				i++;

				fs.Add(vs);
			}
			tr.Add(fs); // add the first section

			var run = new TextCell
			{
				Text = "Run Everything",
				Command = viewModel.RunEverythingCommand,
			};

			table.Root
				.Skip(1)
				.First()
				.Insert(0, run);

			tr.Add(table.Root.Skip(1)); // Skip the first section and add the others

			table.Root = tr;
		}
	}
}
