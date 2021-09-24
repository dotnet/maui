using System.Collections.Generic;
using System.Diagnostics;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Extensions.Configuration;

namespace Maui.Controls.Sample.ViewModels
{
	public class MainViewModel : BaseGalleryViewModel
	{
		readonly IConfiguration _configuration;
		readonly ITextService _textService;

		public MainViewModel(IConfiguration configuration, ITextService textService)
		{
			_configuration = configuration;
			_textService = textService;

			Debug.WriteLine($"Value from config: {_configuration["MyKey"]}");
			Debug.WriteLine($"Value from TextService: {_textService.GetText()}");
		}

		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
#if NET6_0_OR_GREATER
			new SectionModel(typeof(BlazorPage), "Blazor",
				"The BlazorWebView control allow to easily embed Blazor content with native UI."),
#endif

			new SectionModel(typeof(CompatibilityPage), "Compatibility",
				"Functionality available using the compatibility package."),

			new SectionModel(typeof(CorePage), "Core",
				"Application development fundamentals like Shell or Navigation."),

			new SectionModel(typeof(LayoutsPage), "Layouts",
				"Layouts are used to compose user-interface controls into visual structures."),

			new SectionModel(typeof(ControlsPage), "Controls",
				"Controls are the building blocks of cross-platform mobile user interfaces."),

			new SectionModel(typeof(UserInterfacePage), "User Interface Concepts",
				"User interface concepts like Animations, Colors, Fonts and more."),

			new SectionModel(typeof(OthersPage), "Others Concepts",
				"Other options like Graphics."),
		};
	}
}