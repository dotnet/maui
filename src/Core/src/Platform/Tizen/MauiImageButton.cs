using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TButton = Tizen.UIExtensions.ElmSharp.Button;
using TImage = Tizen.UIExtensions.ElmSharp.Image;

namespace Microsoft.Maui.Platform
{
	public class MauiImageButton : Canvas
	{
		TImage _image;
		TButton _button;

		public MauiImageButton(EvasObject parent) : base(parent)
		{
			_image = new TImage(parent);
			_button = new TButton(parent);

			_button.Clicked += OnClicked;
			_button.Pressed += OnPressed;
			_button.Released += OnReleased;
			_button.SetTransparentStyle();

			Children.Add(_image);
			_image.RaiseTop();

			Children.Add(_button);
			_button.SetTransparentStyle();
			_button.RaiseTop();

			LayoutUpdated += OnLayout;
		}

		public TImage ImageView
		{
			get => _image;
		}

		public event EventHandler? Clicked;
		public event EventHandler? Pressed;
		public event EventHandler? Released;

		void OnReleased(object? sender, EventArgs e)
		{
			Released?.Invoke(this, EventArgs.Empty);
		}

		void OnPressed(object? sender, EventArgs e)
		{
			Pressed?.Invoke(this, EventArgs.Empty);
		}

		void OnClicked(object? sender, EventArgs e)
		{
			Clicked?.Invoke(this, EventArgs.Empty);
		}

		void OnLayout(object? sender, LayoutEventArgs e)
		{
			_button.Geometry = Geometry;
			_image.Geometry = Geometry;
		}
	}
}