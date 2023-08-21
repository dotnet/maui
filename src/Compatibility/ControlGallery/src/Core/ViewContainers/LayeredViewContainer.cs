//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Graphics;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class LayeredViewContainer<T> : ViewContainer<T>
		where T : View
	{
		public LayeredViewContainer(Enum formsMember, T view) : base(formsMember, view)
		{
			var layout = new AbsoluteLayout();

			var hiddenButton = new Button
			{
				AutomationId = formsMember + "LayeredHiddenButton",
				Text = "Covered Up"
			};

			layout.Children.Add(hiddenButton);
			AbsoluteLayout.SetLayoutFlags(hiddenButton, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(hiddenButton, new Rect(0, 0, 1, 1));
			layout.Children.Add(view);
			AbsoluteLayout.SetLayoutBounds(view, new Rect(0, 0, 1, 1));
			AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);

			var hiddenLabel = new Label
			{
				AutomationId = formsMember + "LayeredLabel",
				Text = "Hidden Button (Not Clicked)"
			};

			hiddenButton.Clicked += (sender, args) => { hiddenLabel.Text = "Hidden Button (Clicked)"; };

			ContainerLayout = new StackLayout
			{
				AutomationId = formsMember + "Container",
				Padding = 10,
				Children = { TitleLabel, BoundsLabel, layout, hiddenLabel }
			};
		}
	}
}