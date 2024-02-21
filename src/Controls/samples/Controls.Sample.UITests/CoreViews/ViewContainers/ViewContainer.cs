using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	internal class ViewContainer<T> : IViewContainer<T>
		where T : View
	{
		public Label TitleLabel { get; private set; }
		public Label BoundsLabel { get; private set; }
		public T View { get; private set; }

		// May want to override the container layout in subclasses
		public StackLayout ContainerLayout { get; protected set; }

		Layout IViewContainer<T>.ContainerLayout => ContainerLayout;

		public ViewContainer(Enum key, T view)
			: this(key.ToString(), view)
		{
		}

		public ViewContainer(string key, T view)
		{
			view.AutomationId = $"{key}VisualElement";
			View = view;

			TitleLabel = new Label
			{
				Text = $"{key} View"
			};

			BoundsLabel = new Label
			{
				BindingContext = new MultiBindingHack(view)
			};
			BoundsLabel.SetBinding(Label.TextProperty, nameof(MultiBindingHack.LabelWithBounds));

			ContainerLayout = new StackLayout
			{
				AutomationId = $"{key}Container",
				Padding = 10,
				Children = { TitleLabel, BoundsLabel, view }
			};
		}
	}
}