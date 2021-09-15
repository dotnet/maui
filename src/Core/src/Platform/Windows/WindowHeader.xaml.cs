using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui
{
	partial class WindowHeader
	{
		public WindowHeader()
		{
			InitializeComponent();

			this.PrimaryCommands.Add(new AppBarButton() { Label = "primary 1" });
			this.PrimaryCommands.Add(new AppBarButton() { Label = "primary 2" });

			this.SecondaryCommands.Add(new AppBarButton() { Label = "secondary 1" });
			this.SecondaryCommands.Add(new AppBarButton() { Label = "secondary 2" });
		}

		public string Title
		{
			get => title.Text;
			set => title.Text = value;
		}

		public WImageSource TitleIcon
		{
			get => titleIcon.Source;
			set => titleIcon.Source = value;
		}
	}
}
