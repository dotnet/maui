using CollectionViewPerformanceMaui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui;

namespace CollectionViewPerformanceMaui.Platforms.Android.Handlers
{
    public sealed class CardHandler : BorderHandler
    {
        public static void SetupMapper()
        {
            Mapper.AppendToMapping<Card, CardHandler>(nameof(Card.HasElevation), MapHasElevation);
        }

        public static void MapHasElevation(CardHandler handler, Card card)
        {
            if (card.HasElevation)
            {
                handler.PlatformView.Elevation = 10;
            }
        }
    }
}
