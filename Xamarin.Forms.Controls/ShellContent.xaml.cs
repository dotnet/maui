using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls
{
	[Preserve]
	[QueryProperty("Text", "welcome")]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShellContent : ContentPage
	{
		private class MySearchHandler : SearchHandler
		{
			public MySearchHandler()
			{
				ShowsResults = true;
				Placeholder = "Search Me";
			}

			protected override void OnQueryChanged(string oldValue, string newValue)
			{
				base.OnQueryChanged(oldValue, newValue);

				List<string> results = new List<string>();
				for (int i = 0; i < 100; i++)
				{
					results.Add(newValue + i);
				}

				ItemsSource = results;
			}

			protected override void OnQueryConfirmed()
			{
				base.OnQueryConfirmed();
			}

			protected override void OnItemSelected(object item)
			{
				base.OnItemSelected(item);

				ItemsSource = null;
			}
		}

		private string _text;

		public ShellContent()
		{
			InitializeComponent();

			Shell.SetSearchHandler(this, new MySearchHandler());

			//BackgroundColor = Color.Blue;

			_pushButton.Clicked += PushClicked;
			_popButton.Clicked += PopClicked;
			_popToRootButton.Clicked += PopToRootClicked;
			_navButton.Clicked += NavClicked;
			_queryButton.Clicked += QueryClicked;
			_toggleButton.Clicked += ToggleClicked;
			_removeButton.Clicked += RemovedClicked;
			_insertButton.Clicked += InsertClicked;
		}

		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				_mainLabel.Text = _text;
			}
		}

		private void RemovedClicked(object sender, EventArgs e)
		{
			var stack = Navigation.NavigationStack;

			var page = stack[stack.Count - 2];
			Navigation.RemovePage(page);
		}

		private void InsertClicked(object sender, EventArgs e)
		{
			Navigation.InsertPageBefore(new ShellContent(), this);
		}

		private void ToggleClicked(object sender, EventArgs e)
		{
			var shell = Application.Current.MainPage as Shell;
			shell.FlyoutIsPresented = !shell.FlyoutIsPresented;
		}

		private async void QueryClicked(object sender, EventArgs e)
		{
			var shell = Application.Current.MainPage as Shell;
			await shell.GoToAsync("app:///s/apps/movies/shellcontent?welcome=helloworld!");
		}

		private async void NavClicked(object sender, EventArgs e)
		{
			var shell = Application.Current.MainPage as Shell;
			await shell.GoToAsync("app:///s/apps/movies/buttongallery");
		}

		private async void PopToRootClicked(object sender, EventArgs e)
		{
			await Navigation.PopToRootAsync();
		}

		private async void PopClicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}

		private async void PushClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ShellContent()
			{
				Text = Text + "1"
			});
		}
	}
}