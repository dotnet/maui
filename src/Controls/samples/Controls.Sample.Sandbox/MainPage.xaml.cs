using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			BindingContext = new MyViewModel();
		}
	}

	public class MyViewModel : BaseViewModel
	{
		string _recipeName;
		string _ingredients;
		string _recipeBody;

		public MyViewModel()
		{
			RecipeName = "Korean egg roll";
			Ingredients = "1 scallion.\n" +
					"8 eggs\n" +
                    "canola oil\n" +
                    "1 tbsp salt\n" +
                    "1 tbsp pepper\n";
			RecipeBody = "1. Grab a bowl, and add the scallion (chopped), eggs (cracked), salt, pepper.\n" +
					"2. Thoroughly beat the egg mixture.\n" +
					"3. Heat the pan on the stove, and add 3 turns of oil.\n" +
					"4. Once pan is ready/warm, pour in the egg mixture; if using 8 eggs, pour in only half.\n" +
					"5. Tilt the pan around so egg mixture fills the pan.\n" +
					"6. Begin folding the egg over from the edge of the pan.\n" +
					"7. Continually fold egg over while pushing uncooked egg to the other side of the pan and shaping it.\n" +
					"8. Once completely folded, remove from pan.\n" +
					"9. Repeat steps 3 - 8 if there is remaining egg mixture.\n" +
					"10. Cut the 달걀말이 into pieces.";
		}

		public string RecipeName
		{
			get => _recipeName;
			set => SetProperty(ref _recipeName, value);
		}

		public string Ingredients
		{
			get => _ingredients;
			set => SetProperty(ref _ingredients, value);
		}

		public string RecipeBody
		{
			get => _recipeBody;
			set => SetProperty(ref _recipeBody, value);
		}

	}

	public class BaseViewModel : INotifyPropertyChanged
	{
		bool isBusy = false;
		public bool IsBusy
		{
			get { return isBusy; }
			set { SetProperty(ref isBusy, value); }
		}

		string title = string.Empty;
		public string Title
		{
			get { return title; }
			set { SetProperty(ref title, value); }
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;

			changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}