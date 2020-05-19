using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Diagnostics;
using System.ComponentModel;

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

				if (images[0].Rect.Height < 50 || images[0].Rect.Width < 50)
					return null;

				return images;
			});

			Assert.IsNotNull(monkeyImages);
			Assert.GreaterOrEqual(monkeyImages[0].Rect.Height, 50);
			Assert.GreaterOrEqual(monkeyImages[0].Rect.Width, 50);
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
				var json = await Client.GetStringAsync("https://montemagno.com/monkeys.json");
				Monkey[] monkeys = Monkey.FromJson(json);

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