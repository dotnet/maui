
namespace Maui.Controls.Sample
{
	class CollectionView2 : CollectionView { }

	class CarouselView2 : CarouselView { }

	public static partial class CollectionViewHostBuilderExtensions
	{
		/// <summary>
		/// Configure the .NET MAUI app to listen for fold-related events
		/// in the Android lifecycle. Ensures <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneView"/>
		/// can detect and layout around a hinge or screen fold.
		/// </summary>
		/// <remarks>
		/// Relies on Jetpack Window Manager to detect and respond to
		/// foldable device features and capabilities.
		/// </remarks>
		public static MauiAppBuilder ConfigureCollectionViewHandlers(this MauiAppBuilder builder)
		{

#if IOS || MACCATALYST
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CollectionView2, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
				handlers.AddHandler<CarouselView2, Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
			});
#endif

			return builder;
		}
	}
}