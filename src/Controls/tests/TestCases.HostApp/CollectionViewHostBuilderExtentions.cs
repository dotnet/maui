
#if IOS || MACCATALYST
using Foundation;
#endif

namespace Maui.Controls.Sample
{

	class CollectionView1 : CollectionView { }
	class CollectionView2 : CollectionView { }


	class CarouselView1 : CarouselView { }
	class CarouselView2 : CarouselView { }

	public static partial class CollectionViewHostBuilderExtentions
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
				bool cv2Handlers = false;
				foreach (var en in NSProcessInfo.ProcessInfo.Environment)
				{
					if ($"{en.Key}" == "TEST_CONFIGURATION_ARGS")
					{
						cv2Handlers = $"{en.Value}".Contains("CollectionView2", StringComparison.OrdinalIgnoreCase);
						break;
					}
				}

				if (cv2Handlers)
				{
					Console.WriteLine($"Using CollectionView2 handlers");
					handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
					handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
				}
				else
				{
					Console.WriteLine($"Using CollectionView handlers");
					handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler>();
					handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, Microsoft.Maui.Controls.Handlers.Items.CarouselViewHandler>();
				}

				handlers.AddHandler<CollectionView1, Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler>();
				handlers.AddHandler<CarouselView1, Microsoft.Maui.Controls.Handlers.Items.CarouselViewHandler>();


				handlers.AddHandler<CollectionView2, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
				handlers.AddHandler<CarouselView2, Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
			});
#endif

			return builder;
		}
	}
}