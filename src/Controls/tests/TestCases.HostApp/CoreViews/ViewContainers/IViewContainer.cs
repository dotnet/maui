namespace Maui.Controls.Sample
{
	internal interface IViewContainer<out T>
		where T : View
	{
		Label TitleLabel { get; }

		Label BoundsLabel { get; }

		T View { get; }

		Layout ContainerLayout { get; }
	}
}
