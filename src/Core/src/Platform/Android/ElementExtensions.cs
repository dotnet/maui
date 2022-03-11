using System;
using Android.App;
using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static partial class ElementExtensions
	{
		public static AView ToContainerView(this IElement view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };
	}
}