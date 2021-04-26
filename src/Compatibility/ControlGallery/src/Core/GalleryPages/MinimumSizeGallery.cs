using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class MinimumSizeGallery : ContentPage
	{
		public MinimumSizeGallery()
		{
			Content = new StackLayout
			{
				Children = {
					new Button { Text = "Big Button", HeightRequest = 300 },
					new TableView {
						Intent = TableIntent.Settings,
						Root = new TableRoot {
							new TableSection ("SECTION 1") {
								new TextCell { Text = "Cell 1", Detail = "Red Text", TextColor = Colors.Red },
								new TextCell { Text = "Detail Red", Detail = "Detail", DetailColor = Colors.Red },
								new TextCell { Text = "Cell 3" },
								new TextCell { Text = "Cell 4" }
							},
							new TableSection ("Section 2") {
								new TextCell { Text = "Cell 1" },
								new TextCell { Text = "Cell 2" },
								new TextCell { Text = "Cell 3" },
								new TextCell { Text = "Cell 4" }
							},
							new TableSection ("Section 3") {
								new TextCell { Text = "Cell 1" },
								new TextCell { Text = "Cell 2" },
								new TextCell { Text = "Cell 3" },
								new TextCell { Text = "Cell 4" }
							},
							new TableSection ("Section 4") {
								new TextCell { Text = "Cell 1" },
								new TextCell { Text = "Cell 2" },
								new TextCell { Text = "Cell 3" },
								new TextCell { Text = "Cell 4 Last" }
							}
						}
					}
				}
			};
		}
	}
}
