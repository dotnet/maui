using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class XamlApp : Application
	{
		public XamlApp(IServiceProvider services, ITextService textService)
		{
			InitializeComponent();

			Services = services;

			Debug.WriteLine($"The .NET Purple color is {Resources["DotNetPurple"]}");
			Debug.WriteLine($"The injected text service had a message: '{textService.GetText()}'");

			Device.StartTimer(TimeSpan.FromSeconds(3), () => {
				var window = Windows.FirstOrDefault();
				Debug.WriteLine($"The first window is: '{window.Title}'");
				return false;
			});
		}

		public IServiceProvider Services { get; }

		public override IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			return Services.GetRequiredService<IWindow>();
		}
	}
}