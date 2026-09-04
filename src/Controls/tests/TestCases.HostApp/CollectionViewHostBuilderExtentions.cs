namespace Maui.Controls.Sample
{
	class CollectionView2 : CollectionView { }

	class CarouselView2 : CarouselView { }

	public static partial class CollectionViewHostBuilderExtensions
	{
		/// <summary>
		/// Registers explicit Items2 handlers for the test-only CollectionView2 and CarouselView2 controls.
		/// </summary>
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