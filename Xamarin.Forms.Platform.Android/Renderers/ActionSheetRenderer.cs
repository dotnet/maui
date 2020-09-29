using System;
using System.ComponentModel;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Internals;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	[Obsolete("ActionSheetRenderer is obsolete as of version 1.3.2. ActionSheet now uses default implementation.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActionSheetRenderer : Dialog, AView.IOnClickListener
	{
		readonly ActionSheetArguments _arguments;
		readonly LinearLayout _layout;

		internal ActionSheetRenderer(ActionSheetArguments actionSheetArguments) : base(Forms.Context)
		{
			_arguments = actionSheetArguments;
			_layout = new LinearLayout(Context);
		}

		void AView.IOnClickListener.OnClick(AView v)
		{
			var button = (AButton)v;
			_arguments.SetResult(button.Text);
			Hide();
		}

		public override void Cancel()
		{
			base.Cancel();
			_arguments.SetResult(null);
		}

		public override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			Window.SetGravity(GravityFlags.CenterVertical);
			Window.SetLayout(-1, -2);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetCanceledOnTouchOutside(true);

			_layout.Orientation = Orientation.Vertical;

			using (var layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent))
				SetContentView(_layout, layoutParams);

			if (_arguments.Destruction != null)
			{
				AButton destruct = AddButton(_arguments.Destruction);
				destruct.Background.SetColorFilter(new Color(1, 0, 0, 1).ToAndroid(), PorterDuff.Mode.Multiply);
			}

			foreach (string button in _arguments.Buttons)
				AddButton(button);

			if (_arguments.Cancel != null)
			{
				AButton cancel = AddButton(_arguments.Cancel);
				cancel.Background.SetColorFilter(new Color(0.5, 0.5, 0.5, 1).ToAndroid(), PorterDuff.Mode.Multiply);
			}

			SetTitle(_arguments.Title);
		}

		AButton AddButton(string name)
		{
			var button = new AButton(Context) { Text = name };
			button.SetOnClickListener(this);

			_layout.AddView(button);

			return button;
		}
	}
}