using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class ToolbarGallery : ContentPage
	{
		readonly Toolbar _toolbar;
		readonly StackLayout _stack;

		public ToolbarGallery ()
		{
			var label = new Label {
				Text = "Click the toolbar"
			};

			Content = _stack = new StackLayout ();
			_stack.Children.Add (label);

			foreach (string name in new[] { "One", "Two", "Three", "Four" }) {
				var toolbarItem = new ToolbarItem (name, null, delegate {
					label.Text = "Activated: " + name;
				}, ToolbarItemOrder.Secondary);
				ToolbarItems.Add (toolbarItem);
			}

			var imagePrimaryItem = new ToolbarItem (null, "menuIcon.png", () => label.Text = "Activated: Primary Image 1", ToolbarItemOrder.Primary);
			var imagePrimaryItemWithTitle = new ToolbarItem ("Primary", "menuIcon.png", () => label.Text = "Activated: Primary Image 2", ToolbarItemOrder.Primary);
			var imageItem = new ToolbarItem (null, "seth.png", () => label.Text = "Activated: Secondary Image 1", ToolbarItemOrder.Secondary);
			var imageItemWithTitle = new ToolbarItem ("Secondary", "seth.png", () => label.Text = "Activated: Secondary Image 2", ToolbarItemOrder.Secondary);
			ToolbarItems.Add (imagePrimaryItem);
			ToolbarItems.Add (imagePrimaryItemWithTitle);
			ToolbarItems.Add (imageItem);
			ToolbarItems.Add (imageItemWithTitle);
		}
	}
}
