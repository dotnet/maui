using System.Collections.Generic;
using System.Linq;
using Maui.Controls.Sample.Services;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.ViewModel
{
	public class MainPageViewModel : ViewModelBase
	{
		private readonly IConfiguration Configuration;
		ITextService textService;

		public MainPageViewModel() : this(new ITextService[] { App.Current.Services.GetService<ITextService>() })
		{
		}

		public MainPageViewModel(IEnumerable<ITextService> textServices)
		{
			Configuration = App.Current.Services.GetService<IConfiguration>();

			//var logger = App.Current.Services.GetService<ILogger<MainPageViewModel>>();

			//logger.LogInformation("hello");

			textService = textServices.FirstOrDefault();
			Text = textService.GetText();
		}

		//public MainPageViewModel(ITextService textService)
		//{
		//	Text = textService.GetText();
		//}

		string _text;
		public string Text
		{
			get => _text;
			set => SetProperty(ref _text, value);
		}
	}
}
