using System;
using MauiSampleApp.Services;
using Microsoft.Extensions.Configuration;

namespace MauiSampleApp.ViewModel
{
	public class MainPageViewModel : ViewModelBase
	{
		readonly IConfiguration _configuration;
		readonly ITextService _textService;
		string _text;

		public MainPageViewModel(IConfiguration configuration, ITextService textService)
		{
			_configuration = configuration;
			_textService = textService;

			Console.WriteLine($"Value from config: {_configuration["MyKey"]}");

			Text = _textService.GetText();
		}

		public string Text
		{
			get => _text;
			set => SetProperty(ref _text, value);
		}
	}
}