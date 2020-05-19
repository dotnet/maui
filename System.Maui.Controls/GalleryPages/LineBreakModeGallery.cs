using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class LineBreakModeGallery : ContentPage
	{
		public LineBreakModeGallery ()
		{
			var label = new Label {
				Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
			};

			Content = new ScrollView {
				Content = new StackLayout {
					Spacing = 0,
					Children = {
						MakeLineBreakModeButton ("CharacterWrap", label, LineBreakMode.CharacterWrap),
						MakeLineBreakModeButton ("HeadTruncation", label, LineBreakMode.HeadTruncation),
						MakeLineBreakModeButton ("MiddleTruncation", label, LineBreakMode.MiddleTruncation),
						MakeLineBreakModeButton ("NoWrap", label, LineBreakMode.NoWrap),
						MakeLineBreakModeButton ("TailTruncation", label, LineBreakMode.TailTruncation),
						MakeLineBreakModeButton ("WordWrap", label, LineBreakMode.WordWrap),
						label
					}
				}
			};
		}

		static Button MakeLineBreakModeButton (string buttonText, Label label, LineBreakMode lineBreakMode) {
			var button = new Button { 
				Text = buttonText,
				Command = new Command (() => label.LineBreakMode = lineBreakMode)
			};
			return button;
		}
	}
}
