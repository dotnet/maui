using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, AView>
	{
		protected override AView CreateNativeView()
		{
			throw new NotImplementedException();
		}
	}
}
