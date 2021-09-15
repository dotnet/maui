using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui
{
	[Bindable]
	public class MauiNavigationView : NavigationView
	{
		public WGrid? ContentTopPadding { get; private set; }
		public WGrid? PaneToggleButtonGrid { get; private set; }
		public ContentControl? HeaderContent { get; private set; }
		WThickness? DefaultHeaderContentMargin { get; set; }
		WThickness? HeaderContentMargin { get; set; }

		public MauiNavigationView()
		{
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PaneToggleButtonGrid = (WGrid)GetTemplateChild("PaneToggleButtonGrid");
			ContentTopPadding = (WGrid)GetTemplateChild("ContentTopPadding");
			HeaderContent = (ContentControl)GetTemplateChild("HeaderContent");
			DefaultHeaderContentMargin = HeaderContent.Margin;
			HeaderContentMargin = new WThickness(
				0,
				0,
				DefaultHeaderContentMargin.Value.Right,
				DefaultHeaderContentMargin.Value.Bottom);
		}

		internal async void UpdateBarBackgroundBrush(WBrush? brush)
		{
			if (Header is not WindowHeader windowHeader)
				return;

			windowHeader.Background = brush;

			if (PaneToggleButtonGrid != null)
			{
				PaneToggleButtonGrid.Background = windowHeader.Background;
			}

			if (ContentTopPadding != null)
			{
				ContentTopPadding.Background = windowHeader.Background;
			}

			await Task.Delay(1000);
			if (HeaderContent != null && HeaderContentMargin != null)
				HeaderContent.Margin = HeaderContentMargin.Value;
		}
	}
}
