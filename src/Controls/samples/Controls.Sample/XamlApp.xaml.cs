using System;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public partial class XamlApp : Application
	{
		public XamlApp(IServiceProvider services, ITextService textService)
		{
			InitializeComponent();

			Services = services;

			Console.WriteLine($"The .NET Purple color is {Resources["DotNetPurple"]}");
			Console.WriteLine($"The injected text service had a message: '{textService.GetText()}'");
		}

		public IServiceProvider Services { get; }

		public override IWindow CreateWindow(IActivationState activationState)
		{
			Forms.Init(activationState);

			return Services.GetRequiredService<IWindow>();
		}
	}
}