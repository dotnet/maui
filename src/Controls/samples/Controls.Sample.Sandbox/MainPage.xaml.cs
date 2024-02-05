using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			var cat = new TableView
			{
				Root = new TableRoot {

					new TableSection("hi there") {
						new ViewCell {
							View = new StackLayout {
								Children = {
									new Label {
										Text = "Custom Slider View:"
									},
								}
							}
						},
						//new EntryCell { Text = "entry cell" }
					}
				}
			};

			Content = 
			/*new TableView
					{
						Root = new TableRoot
						{
							new TableSection
							{
								//new SwitchCell { Text = "switch cell", On = true },
								new EntryCell { Text = "entry cell" }
							}
						}
					};*/
			/*new VerticalStackLayout(){
				cat
			}*/
			cat
			;
		}
	}
}