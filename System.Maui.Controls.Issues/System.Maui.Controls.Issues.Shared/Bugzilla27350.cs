using System;

using Xamarin.Forms.CustomAttributes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 27350, "Binding throws Null Pointer Exception when Updating Tab")]
	public class Bugzilla27350 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var btn = new Button { Text = "next main" };
			btn.Clicked += async (object sender, EventArgs e) => await Navigation.PushAsync (new MainPage1 ());
			Content = btn;
		}

		class RecipeViewModel
		{
			public Recipe Recipe { get; set; }

			public ObservableCollection<RecipeGroup> RecipeGroups { get; set; }

#pragma warning disable 1998 // considered for removal
			public async Task LoadRecipesAsync ()
#pragma warning restore 1998
			{
				var groups = new  ObservableCollection<RecipeGroup> ();
				groups.Add (new RecipeGroup { Title = "Teste 1" });
				groups.Add (new RecipeGroup { Title = "Teste 2" });
				groups.Add (new RecipeGroup { Title = "Teste 3" });
				groups.Add (new RecipeGroup { Title = "Teste 4" });
				groups.Add (new RecipeGroup { Title = "Teste 5" });
				groups.Add (new RecipeGroup { Title = "Teste 6" });
				groups.Add (new RecipeGroup { Title = "Teste 4" });
				groups.Add (new RecipeGroup { Title = "Teste 5" });
				groups.Add (new RecipeGroup { Title = "Teste 6" });

				RecipeGroups = groups;
			}
		}

		class Recipe
		{
			public string ID { get; set; }

			public string Title { get; set; }

			public string Subtitle { get; set; }

			public string Description { get; set; }

			public string ImagePath { get; set; }

			public string TileImagePath { get; set; }

			public int PrepTime { get; set; }

			public string Directions { get; set; }

			public List<string> Ingredients { get; set; }
		}

		class RecipeGroup : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged (string caller)
			{
				var handler = PropertyChanged;
				if (handler != null) {
					handler (this, new PropertyChangedEventArgs (caller));
				}
			}

			public string ID { get; set; }

			public string Title {
				get{ return _title; }
				set {
					_title = value;
					OnPropertyChanged ("Title");
				}
			}

			string _title;

			public string Subtitle { get; set; }

			public string ImagePath { get; set; }

			public string GroupImagePath { get; set; }

			public string Description { get; set; }

			public List<Recipe> Recipes { get; set; }
		}

		class MainPage1 : TabbedPage
		{
			public MainPage1 ()
			{
				ItemTemplate = new DataTemplate (() => {
					var page = new ContentPage ();
					page.SetBinding (TitleProperty, new Binding ("Title"));
					var btn = new Button { Text = "change", Command = new Command (() => {
							(page.BindingContext as RecipeGroup).Title = "we changed";
						})
					};
					var btn1 = new Button { Text = "null", Command = new Command (() => {
							(page.BindingContext as RecipeGroup).Title = null;
						})
					};
					page.Content = new StackLayout { Children = { btn, btn1 } };
					return page;
				});
				SetBinding (ItemsSourceProperty, new Binding ("RecipeGroups"));
			}

			protected override async void OnAppearing ()
			{
				base.OnAppearing ();

				if (BindingContext == null)
					BindingContext = await GetRecipeViewModelAsync ();
			}

			RecipeViewModel _rvm;

			public async Task<RecipeViewModel> GetRecipeViewModelAsync ()
			{
				if (_rvm == null) {
					_rvm = new RecipeViewModel ();
				} else {
					_rvm.RecipeGroups.Clear ();
				}

				await _rvm.LoadRecipesAsync ();

				return _rvm;
			}
		}
	}
}
