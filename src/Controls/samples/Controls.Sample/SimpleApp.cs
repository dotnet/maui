using System;
using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.Pages;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public class SimpleApp : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
			Forms.Init(activationState);

			return new Window
			{
				//Page = new SimplePage()
				Page = new SimpleButtonIsPage
				{
					Text = "Yup, very simple!",
					FontFamily = "Dokdo",
				}
			};
		}

		class SimpleButtonIsPage : Button, IPage
		{
			public IView View { get => this; set => throw new NotImplementedException(); }
		}
	}
}