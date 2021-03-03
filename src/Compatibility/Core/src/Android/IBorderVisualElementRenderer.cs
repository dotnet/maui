using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IBorderVisualElementRenderer
	{
		float ShadowRadius { get; }
		float ShadowDx { get; }
		float ShadowDy { get; }
		AColor ShadowColor { get; }
		bool UseDefaultPadding();
		bool UseDefaultShadow();
		bool IsShadowEnabled();
		VisualElement Element { get; }
		AView View { get; }
		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
	}
}