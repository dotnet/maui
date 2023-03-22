using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class FramePage
	{
		public FramePage()
		{
			InitializeComponent();
		}

		void OnHasShadowButtonClicked(object sender, System.EventArgs e)
		{
			HasShadowFrame.HasShadow = !HasShadowFrame.HasShadow;
		}

		void OnAddContentButtonClicked(object sender, EventArgs e)
		{
			ContentFrame.Content = new Label
			{
				Text = "Content",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = 12
			};
		}

		void OnReplaceContentButtonClicked(object sender, EventArgs e)
		{
			if (ContentFrame.Content == null)
				return;

			ContentFrame.Content = new Label
			{
				Text = "Updated Content",
				BackgroundColor = Colors.Red,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = 12
			};
		}

		void OnRemoveContentButtonClicked(object sender, EventArgs e)
		{
			ContentFrame.Content = null;
		}
	}
}