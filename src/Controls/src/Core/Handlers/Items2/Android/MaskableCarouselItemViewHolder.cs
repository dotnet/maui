#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Google.Android.Material.Carousel;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A <see cref="Items.SelectableViewHolder"/> whose root <see cref="AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder.ItemView"/>
/// is a <see cref="MaskableFrameLayout"/>, satisfying the Material <see cref="CarouselLayoutManager"/>
/// requirement that every direct RecyclerView child must be a <see cref="MaskableFrameLayout"/>.
///
/// MAUI content is rendered inside an inner <see cref="Items.ItemContentView"/> that is added
/// as the sole child of the <see cref="MaskableFrameLayout"/>.
/// </summary>
internal sealed class MaskableCarouselItemViewHolder : Items.SelectableViewHolder
{
    readonly Items.ItemContentView _itemContentView;
    readonly DataTemplate _template;
    DataTemplate _selectedTemplate;
    bool _logicalChildAdded;

    public Controls.View View { get; private set; }

    public MaskableCarouselItemViewHolder(
        MaskableFrameLayout maskableRoot,
        Items.ItemContentView itemContentView,
        DataTemplate template)
        : base(maskableRoot, isSelectionEnabled: false)
    {
        _itemContentView = itemContentView;
        _template = template;
    }

    public void Bind(object itemBindingContext, ItemsView itemsView)
    {
        // _template can be null in selector scenarios where GetItemViewType hasn't yet
        // populated the adapter's view-type cache. Fall back to the current ItemTemplate
        // and resolve via SelectDataTemplate (which handles both selectors and plain templates).
        var rootTemplate = _template ?? itemsView?.ItemTemplate;
        if (rootTemplate is null)
        {
            return;
        }

        var template = rootTemplate.SelectDataTemplate(itemBindingContext, itemsView);
        bool templateChanging = template != _selectedTemplate;

        if (templateChanging)
        {
            // Tear down the previous content (if any) before realizing the new template.
            if (View is not null)
            {
                itemsView.RemoveLogicalChild(View);
                _logicalChildAdded = false;
            }
            _itemContentView.Recycle();

            var content = template.CreateContent();
            View = content as Controls.View
                ?? throw new InvalidOperationException($"{template} could not be created from {content}");

            View.BindingContext = itemBindingContext;
            PropertyPropagationExtensions.PropagatePropertyChanged(null, View, itemsView);
            _itemContentView.RealizeContent(View, itemsView);
            _selectedTemplate = template;
        }
        else if (View is not null)
        {
            // Same template, new data — refresh binding context and re-propagate parent values
            // so visual states / inherited bindings update on rebind.
            View.BindingContext = itemBindingContext;
            PropertyPropagationExtensions.PropagatePropertyChanged(null, View, itemsView);
        }

        if (View is not null && !_logicalChildAdded)
        {
            itemsView.AddLogicalChild(View);
            _logicalChildAdded = true;
        }
    }

    public void Recycle(ItemsView itemsView)
    {
        if (View is null)
        {
            return;
        }

        if (_logicalChildAdded && itemsView is not null)
        {
            itemsView.RemoveLogicalChild(View);
        }

        _itemContentView.Recycle();

        View = null;
        _selectedTemplate = null;
        _logicalChildAdded = false;
    }
}
