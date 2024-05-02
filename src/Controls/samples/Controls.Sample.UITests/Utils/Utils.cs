using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public static class Utils
	{
		public static T With<T>(this T that, Action<T> action)
		{
			action(that);
			return that;
		}
	}
}
