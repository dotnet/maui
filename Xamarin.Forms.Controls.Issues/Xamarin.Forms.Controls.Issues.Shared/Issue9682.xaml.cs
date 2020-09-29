using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9682, "[iOS] URI Images no longer loading",
		PlatformAffected.iOS)]
	public partial class Issue9682 : TestContentPage
	{
		public Issue9682()
		{
#if APP
			InitializeComponent();
#endif

		}

		protected override void Init()
		{
			BindingContext = new MonkeysViewModel();
		}

#if UITEST
		[Test, NUnit.Framework.Category(UITestCategories.Image)]
		public void MonkiesShouldLoad()
		{
			RunningApp.WaitForElement("MonkeyLoadButton");
			RunningApp.Tap("MonkeyLoadButton");
			RunningApp.WaitForElement("monkeysLoaded");

			var monkeyImages = RunningApp.QueryUntilPresent(() =>
			{
				var images = RunningApp.WaitForElement("MonkeyImages");

				if (images[0].Rect.Height < 20 || images[0].Rect.Width < 20)
					return null;

				return images;
			});

			monkeyImages = monkeyImages ?? RunningApp.WaitForElement("MonkeyImages");

			Assert.IsNotNull(monkeyImages);
			Assert.GreaterOrEqual(monkeyImages[0].Rect.Height, 20);
			Assert.GreaterOrEqual(monkeyImages[0].Rect.Width, 20);
		}
#endif
	}


	public partial class Monkey
	{
		public string Name { get; set; }

		public string Location { get; set; }

		public string Details { get; set; }

		public string Image { get; set; }

		public long Population { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public static Monkey[] FromJson(string json) => JsonConvert.DeserializeObject<Monkey[]>(json);
	}

	public class MonkeysViewModel : INotifyPropertyChanged
	{
		HttpClient httpClient;
		bool _isBusy;
		bool _isLoaded;
		HttpClient Client => httpClient ?? (httpClient = new HttpClient());

		public Command GetMonkeysCommand { get; }
		public ObservableCollection<Monkey> Monkeys { get; }

		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				_isBusy = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
			}
		}

		public bool IsLoaded
		{
			get => _isLoaded;
			set
			{
				_isLoaded = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoaded)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public MonkeysViewModel()
		{
			Monkeys = new ObservableCollection<Monkey>();
			GetMonkeysCommand = new Command(async () => await GetMonkeysAsync());
			IsBusy = false;
			IsLoaded = false;
		}


		async Task GetMonkeysAsync()
		{
			if (IsBusy)
				return;

			IsLoaded = false;
			try
			{
				IsBusy = true;
				await Task.Delay(500);
				Monkey[] monkeys = new[]
				{
					new Monkey()
					{
						Details = "Climbs high buildings and enjoys swatting at planes",
						Latitude = 42,
						Longitude = 42,
						Location = "Empire State Building",
						Name = "King Kong",
						Image = "https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Xamarin.Forms.ControlGallery.Android/Resources/drawable/Fruits.jpg?raw=true"
					},
					new Monkey()
					{
						Details = "Monkeys aren't donkeys, Quit messing with my head!",
						Latitude = 42,
						Longitude = 42,
						Location = "The 90s",
						Name = "Donkey Kong",
						Image = "https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Xamarin.Forms.ControlGallery.Android/Resources/drawable/FlowerBuds.jpg?raw=true"
					},
					new Monkey()
					{
						Details = "Grape Ape, Grape Ape! Grape Ape, Grape Ape! Grape Ape, Grape Ape! Grape Ape, Grape Ape!",
						Latitude = 42,
						Longitude = 42,
						Location = "Sunday Mornings",
						Name = "Grape Ape",
						Image = "https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Xamarin.Forms.ControlGallery.Android/Resources/drawable/games.png?raw=true"
					},
					new Monkey()
					{
						Details = "Pretty but easily persuaded into doing others biddings",
						Latitude = 42,
						Longitude = 42,
						Location = "The Sky",
						Name = "Flying Monkey",
						Image = "https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Xamarin.Forms.ControlGallery.Android/Resources/drawable/gear.png?raw=true"
					},
				};

				Monkeys.Clear();
				foreach (var monkey in monkeys)
					Monkeys.Add(monkey);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
				if (Application.Current?.MainPage == null)
					return;
			}
			finally
			{
				IsBusy = false;
				IsLoaded = true;
			}
		}
	}
}