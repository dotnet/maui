using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using EColor = ElmSharp.Color;

namespace Tizen.UIExtensions.Shell
{
	public interface INavigationView
	{
		EvasObject TargetView { get; }

		EvasObject Header { get; set; }

		EvasObject Footer { get; set; }

		EvasObject Content { get; set; }

		EColor BackgroundColor { get; set; }

		EvasObject BackgroundImage { get; set; }

		event EventHandler<LayoutEventArgs> LayoutUpdated;
	}
}
