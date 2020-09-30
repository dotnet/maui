using System;
using System.Linq.Expressions;

namespace Xamarin.Forms.Controls
{
	internal enum ViewLayoutType
	{
		Normal,
		Layered
	}

	internal class ViewContainer<T>
		where T : View
	{

		public Label TitleLabel { get; private set; }
		public Label BoundsLabel { get; private set; }
		public T View { get; private set; }

		// May want to override the container layout in subclasses
		public StackLayout ContainerLayout { get; protected set; }

		public ViewContainer(Enum formsMember, T view)
		{
			view.AutomationId = formsMember + "VisualElement";
			View = view;

			TitleLabel = new Label
			{
				Text = formsMember + " View"
			};

			BoundsLabel = new Label
			{
				BindingContext = new MultiBindingHack(view)
			};
			BoundsLabel.SetBinding(Label.TextProperty, "LabelWithBounds");

			ContainerLayout = new StackLayout
			{
				AutomationId = formsMember + "Container",
				Padding = 10,
				Children = { TitleLabel, BoundsLabel, view }
			};
		}
	}
}