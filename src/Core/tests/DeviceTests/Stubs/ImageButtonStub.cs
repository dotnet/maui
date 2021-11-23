﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class ImageButtonStub : StubBase, IImageButton, IImageSourcePartEvents, IImageStub
	{
		public Color StrokeColor { get; set; }

		public double StrokeThickness { get; set; }

		public int CornerRadius { get; set; }

		public Aspect Aspect { get; set; }

		public bool IsOpaque { get; set; }

		public IImageSource Source { get; set; }

		public bool IsAnimationPlaying { get; set; }

		public bool IsLoading { get; private set; }

		public IImageSource ImageSource
		{
			get => Source;
			set => Source = value;
		}

		public Color BorderColor { get; set; }

		public double BorderWidth { get; set; }

		public int CornerRadius { get; set; }

		public Thickness Padding { get; set; }

		public event Action LoadingStarted;
		public event Action<bool> LoadingCompleted;
		public event Action<Exception> LoadingFailed;

		public void UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;

		void IImageSourcePartEvents.LoadingCompleted(bool successful) =>
			LoadingCompleted?.Invoke(successful);

		void IImageSourcePartEvents.LoadingFailed(Exception exception) =>
			LoadingFailed?.Invoke(exception);

		void IImageSourcePartEvents.LoadingStarted() =>
			LoadingStarted?.Invoke();


		public event EventHandler Pressed;
		public event EventHandler Released;
		public event EventHandler Clicked;

		void IButton.Pressed() => Pressed?.Invoke(this, EventArgs.Empty);
		void IButton.Released() => Released?.Invoke(this, EventArgs.Empty);
		void IButton.Clicked() => Clicked?.Invoke(this, EventArgs.Empty);
	}
}