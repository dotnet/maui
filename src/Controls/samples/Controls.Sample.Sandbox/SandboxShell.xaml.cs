using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;
using Microsoft.Maui.Controls.Handlers.Compatibility;

namespace Maui.Controls.Sample
{
	public partial class SandboxShell : Shell
	{
		public SandboxShell()
		{
			InitializeComponent();
		}
	}
}