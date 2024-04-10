using System.Threading.Tasks;
using CollectionViewPerformanceMaui.Enums;
using CollectionViewPerformanceMaui.Models;
using CollectionViewPerformanceMaui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Linq;
using System;

namespace CollectionViewPerformanceMaui.ViewModels
{
	public class MyDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate? Card { get; set; }

		public DataTemplate? CardWithShadow { get; set; }

        public DataTemplate? CardWithElevation { get; set; }

        public DataTemplate? CardWithCornerRadius { get; set; }

		public DataTemplate? CardWithBindableLayout { get; set; }

		public DataTemplate? CardWithTapGesture { get; set; }

		public DataTemplate? CardWithGrid { get; set; }

		public DataTemplate? CardWithTheLot { get; set; }

        public DataTemplate? CardWithComplexContent { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
		{
            return ((Data)item).Template switch
            {
                Template.Card => this.Card,
				Template.CardWithShadow => this.CardWithShadow,
				Template.CardWithElevation => this.CardWithElevation,
				Template.CardWithCornerRadius => this.CardWithCornerRadius,
				Template.CardWithBindableLayout => this.CardWithBindableLayout,
				Template.CardWithTapGesture => this.CardWithTapGesture,
				Template.CardWithGrid => this.CardWithGrid,
				Template.CardWithTheLot => this.CardWithTheLot,
				Template.CardWithComplexContent => this.CardWithComplexContent,
                _ => null
			};
		}
	}
	public sealed partial class DataViewModel : ObservableObject
	{
		private readonly IDataService dataService;

		public DataViewModel(IDataService dataService)
		{
			this.dataService = dataService;
		}

		[ObservableProperty]
		private bool isBusy = true;

		public ObservableRangeCollection<Data> Data { get; set; } = new();

        public string CurrentTemplate
            => this.Data.FirstOrDefault()?.Template.ToString()
            ?? "Unknown";

        public async Task InitialiseAsync()
		{
			var data = await this.dataService.GetData();

			this.Data.AddRange(data);

            this.OnPropertyChanged(nameof(CurrentTemplate));

            this.IsBusy = false;
		}

		[RelayCommand]
		private async Task OpenTemplateSwitcher()
		{
			if (Application.Current?.MainPage is null)
			{
				return;
			}

			var result = await Application.Current.MainPage.DisplayActionSheet(
				"Pick template",
				"Cancel",
				null,
				Enum.GetNames(typeof(Template)));

			if (result is null || result == "Cancel")
			{
				return;
			}

			var template = Enum.Parse<Template>(result);

			var data = this.Data.ToList();

			data.ForEach(d => d.Template = template);

			this.Data.ReplaceRange(data);

            this.OnPropertyChanged(nameof(CurrentTemplate));
        }

        [RelayCommand]
		private async Task OpenTapAlert()
		{
			if (Application.Current?.MainPage is null)
			{
				return;
			}

			await Application.Current.MainPage.DisplayAlert(
				"Ahoy",
				"The card was tapped",
				"Close");
		}
	}
}
