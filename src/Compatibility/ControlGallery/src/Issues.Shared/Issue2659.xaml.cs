using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2659, "", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue2659 : ContentPage
	{
		public Issue2659()
		{
			try
			{
				InitializeComponent();
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
		}

		internal void OnSetStyleButtonClicked(object sender, EventArgs args)
		{
			Style style = (Style)Resources["buttonStyle"];
			SetButtonStyle(style);
		}

		internal void OnUnsetStyleButtonClicked(object sender, EventArgs args)
		{
			SetButtonStyle(null);
		}

		internal void OnSetLocalButtonClicked(object sender, EventArgs args)
		{
			EnumerateButtons((Button button) =>
			{
				button.TextColor = Colors.Red;
				button.FontAttributes = FontAttributes.Bold;
			});
		}

		internal void OnClearLocalButtonClicked(object sender, EventArgs args)
		{
			EnumerateButtons((Button button) =>
			{
				button.ClearValue(Button.TextColorProperty);
				button.ClearValue(Button.FontAttributesProperty);
			});
		}

		void SetButtonStyle(Style style)
		{
			EnumerateButtons(button =>
			{
				button.Style = style;
			});
		}

		void EnumerateButtons(Action<Button> action)
		{
			foreach (View view in stackLayout.Children)
				action((Button)view);
		}
	}
#endif
}
