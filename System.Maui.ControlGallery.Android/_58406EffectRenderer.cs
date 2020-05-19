using System;
using System.Diagnostics;
using System.Linq;
using Android.Widget;
using System.Maui;
using System.Maui.Controls.Issues;
using System.Maui.Platform.Android;

[assembly: ExportEffect(typeof(System.Maui.ControlGallery.Android._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace System.Maui.ControlGallery.Android
{
	public class _58406EffectRenderer : PlatformEffect
	{
		protected override void OnAttached()
		{
			var tv = Control as TextView;

			if (tv == null)
			{
				return;
			}

			ReverseText(tv, tv.Text);
			tv.TextChanged += OnTextChanged;
		}

		bool _ignoreNextTextChange;

		void OnTextChanged(object sender, global::Android.Text.TextChangedEventArgs textChangedEventArgs)
		{
			var tv = sender as TextView;

			if (tv == null)
			{
				return;
			}

			if (_ignoreNextTextChange)
			{
				_ignoreNextTextChange = false;
				return;
			}

			_ignoreNextTextChange = true;

			ReverseText(tv, textChangedEventArgs.Text.ToString());
		}

		static void ReverseText(TextView tv, string text)
		{
			var rev = new string(text.Reverse().ToArray());
			tv.Text = rev;
		}

		protected override void OnDetached()
		{
			var tv = Control as TextView;
			if (tv == null)
			{
				return;
			}

			tv.TextChanged -= OnTextChanged;
		}
	}
}