#nullable disable
using Android.Widget;
using Google.Android.Material.Carousel;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A <see cref="Items.SelectableViewHolder"/> for non-templated (plain text) CarouselView items
/// whose root <see cref="AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder.ItemView"/> is a
/// <see cref="MaskableFrameLayout"/>, satisfying the Material <see cref="CarouselLayoutManager"/>
/// requirement that every direct RecyclerView child must be a <see cref="MaskableFrameLayout"/>.
///
/// The item text is rendered by an inner Android.Widget.TextView added as the sole
/// child of the <see cref="MaskableFrameLayout"/>.
/// </summary>
internal sealed class MaskableTextItemViewHolder : Items.SelectableViewHolder
{
    public TextView TextView { get; }

    public MaskableTextItemViewHolder(MaskableFrameLayout maskableRoot, TextView textView)
        : base(maskableRoot, isSelectionEnabled: false)
    {
        TextView = textView;
    }
}
