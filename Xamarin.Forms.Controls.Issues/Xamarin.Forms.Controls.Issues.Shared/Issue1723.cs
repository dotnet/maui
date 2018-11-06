using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1723, "Picker's Items.Clear cause exception", PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Issue1723
		: ContentPage
	{
		Picker _picker = null;

		public Issue1723()
		{
			StackLayout layout = new StackLayout();
			Content = layout;

			_picker = new Picker();
			layout.Children.Add(_picker);

			Button button = new Button();
			button.Clicked += button_Clicked;
			button.Text = "prepare magic";
			layout.Children.Add(button);
		}

		void button_Clicked(object sender, EventArgs e)
		{
			Random r = new Random();

			_picker.Items.Clear();

			for (int j = 0; j < r.Next(10, 30); j++)
			{
				StringBuilder sb = new StringBuilder();
				for (int k = 10; k < r.Next(15, 35); k++)
				{
					sb.Append((char)r.Next(65, 90));
				}
				_picker.Items.Add(sb.ToString());
			}
			_picker.SelectedIndex = r.Next(0, _picker.Items.Count);

			Button button = (Button)sender;
			button.Text = "crash the magic";
		}
	}
}
