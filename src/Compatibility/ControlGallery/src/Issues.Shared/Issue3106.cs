using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3106, "Added LineBreakMode on Button")]
	public class Issue3106 : TestContentPage
	{
		int count;
		const string content = "Welcome to Xamarin.Forms! Welcome to Xamarin.Forms! Welcome to Xamarin.Forms! Welcome to Xamarin.Forms!";
		const string content2 = "Now users can set a line break mode to texts on Button, the default value doesn't affect any user.";

		Button mainButton;
		Button materialButton;
		Label lineBreakModeType;

		protected override void Init()
		{
			mainButton = new Button
			{
				Text = content,
				LineBreakMode = LineBreakMode.WordWrap,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			mainButton.Clicked += MainButton_Clicked;

			materialButton = new Button
			{
				Text = content,
				LineBreakMode = LineBreakMode.WordWrap,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Visual = VisualMarker.Material
			};
			materialButton.Clicked += MaterialButton_Clicked;

			lineBreakModeType = new Label
			{
				Text = LineBreakMode.WordWrap.ToString(),
				VerticalOptions = LayoutOptions.EndAndExpand,
				LineBreakMode = LineBreakMode.WordWrap,
			};
			var layout = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Press the first button to change the LineBreakMode. Press the second button to change the text",
						VerticalOptions = LayoutOptions.StartAndExpand
					},
					mainButton,
					materialButton,
					lineBreakModeType
				}
			};

			Content = layout;
		}

		void MaterialButton_Clicked(object sender, EventArgs e)
		{
			if (materialButton.Text.Equals(content2))
				materialButton.Text = mainButton.Text = content;
			else
				materialButton.Text = mainButton.Text = content2;
		}

		void MainButton_Clicked(object sender, EventArgs e)
		{
			materialButton.LineBreakMode = mainButton.LineBreakMode = SelectLineBreakMode();
			lineBreakModeType.Text = mainButton.LineBreakMode.ToString();
		}

		LineBreakMode SelectLineBreakMode()
		{
			count++;
			switch (count)
			{
				case 1:
					return LineBreakMode.CharacterWrap;
				case 2:
					return LineBreakMode.HeadTruncation;
				case 3:
					return LineBreakMode.MiddleTruncation;
				case 4:
					return LineBreakMode.NoWrap;
				case 5:
					return LineBreakMode.TailTruncation;
				default:
					count = 0;
					return LineBreakMode.WordWrap;
			}
		}
	}
}