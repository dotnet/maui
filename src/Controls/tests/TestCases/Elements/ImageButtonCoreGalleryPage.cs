using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

internal class ImageButtonCoreGalleryPage : CoreGalleryPage<ImageButton>
{
	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(ImageButton element)
	{
		element.Source = "small_dotnet_bot.png";
	}

	protected override void Build()
	{
		base.Build();

		// TODO:
		// Tests to bring over from old gallery:
		// - Source,
		// - IsOpaque,
		// - IsLoading,
		// - Image,
		// Other things that may need to be added:
		// - Adding/changing image at runtime

		AddState(Test.ImageButton.Aspect,
			() => new ImageButton
			{
				Background = Brush.Red,
				Command = new Command(() => { }),
				HeightRequest = 100,
			},
			btn =>
			{
				btn.Aspect = btn.Aspect switch
				{
					Aspect.AspectFit => Aspect.AspectFill,
					Aspect.AspectFill => Aspect.Fill,
					Aspect.Fill => Aspect.Center,
					_ => Aspect.AspectFit,
				};
			});

		Add(Test.ImageButton.Aspect_AspectFill, () =>
			new ImageButton
			{
				Background = Brush.Red,
				Aspect = Aspect.AspectFill
			});

		Add(Test.ImageButton.Aspect_AspectFit, () =>
			new ImageButton
			{
				Background = Brush.Red,
				Aspect = Aspect.AspectFit
			});

		Add(Test.ImageButton.Aspect_Fill, () =>
			new ImageButton
			{
				Background = Brush.Red,
				Aspect = Aspect.Fill
			});

		Add(Test.ImageButton.Aspect_Center, () =>
			new ImageButton
			{
				Background = Brush.Red,
				Aspect = Aspect.Center
			});

		Add(Test.ImageButton.BorderColor, () =>
			new ImageButton
			{
				BorderColor = Colors.Red,
				BorderWidth = 1,
			});

		Add(Test.ImageButton.CornerRadius, () =>
			new ImageButton
			{
				BorderColor = Colors.Red,
				CornerRadius = 20,
				BorderWidth = 1,
			});

		Add(Test.ImageButton.BorderWidth, () =>
			new ImageButton
			{
				BorderColor = Colors.Red,
				BorderWidth = 15,
			});

		Add(Test.ImageButton.BorderColor_WithBackground, () =>
			new ImageButton
			{
				Background = Brush.Green,
				BorderColor = Colors.Red,
				BorderWidth = 1,
			});

		Add(Test.ImageButton.CornerRadius_WithBackground, () =>
			new ImageButton
			{
				Background = Brush.Green,
				BorderColor = Colors.Red,
				CornerRadius = 20,
				BorderWidth = 1,
			});

		Add(Test.ImageButton.BorderWidth_WithBackground, () =>
			new ImageButton
			{
				Background = Brush.Green,
				BorderColor = Colors.Red,
				BorderWidth = 15,
			});

		AddEvent(Test.ImageButton.Clicked,
			() => new ImageButton(),
			container => container.View.Clicked += (_, _) => container.EventFired());

		AddEvent(Test.ImageButton.Pressed,
			() => new ImageButton(),
			container => container.View.Pressed += (_, _) => container.EventFired());

		AddEvent(Test.ImageButton.Command,
			() => new ImageButton(),
			container => container.View.Command = new Command(() => container.EventFired()));

		AddState(Test.ImageButton.Padding,
			() => new ImageButton
			{
				Background = Brush.Red,
				Padding = new Thickness(20, 30, 60, 15)
			},
			btn => btn.Padding = btn.Padding == Thickness.Zero
				? new Thickness(20, 30, 60, 15)
				: Thickness.Zero);

		AddState(Test.ImageButton.Padding_Add,
			() => new ImageButton
			{
				Background = Brush.Red,
			},
			btn => btn.Padding = btn.Padding == Thickness.Zero
				? new Thickness(20, 30, 60, 15)
				: Thickness.Zero);
	}

	void Add(Test.ImageButton test, Func<ImageButton> ctor, Action<ImageButton> action = null)
	{
		var container = new ViewContainer<ImageButton>(test, ctor());
		InitializeElement(container.View);
		if (action is not null)
		{
			container.View.Command = new Command(() => action(container.View));
		}
		Add(container);
	}

	void AddState(Test.ImageButton test, Func<ImageButton> ctor, Action<ImageButton> action)
	{
		var container = new StateViewContainer<ImageButton>(test, ctor());
		InitializeElement(container.View);
		container.StateChangeButton.Command = new Command(() => action(container.View));
		Add(container);
	}

	void AddEvent(Test.ImageButton test, Func<ImageButton> ctor, Action<EventViewContainer<ImageButton>> action)
	{
		var container = new EventViewContainer<ImageButton>(test, ctor());
		InitializeElement(container.View);
		action(container);
		Add(container);
	}
}
