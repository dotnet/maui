#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class TemplatedItemViewHolder : SelectableViewHolder
	{
		readonly ItemContentView _itemContentView;
		readonly DataTemplate _template;
		DataTemplate _selectedTemplate;

		public View View { get; private set; }

		public TemplatedItemViewHolder(ItemContentView itemContentView, DataTemplate template,
			bool isSelectionEnabled = true) : base(itemContentView, isSelectionEnabled)
		{
			_itemContentView = itemContentView;
			_template = template;
		}

		protected override void OnSelectedChanged()
		{
			base.OnSelectedChanged();

			if (View == null)
			{
				return;
			}

			VisualStateManager.GoToState(View, IsSelected
				? VisualStateManager.CommonStates.Selected
				: VisualStateManager.CommonStates.Normal);
		}

		public void Recycle(ItemsView itemsView)
		{
			if (View == null)
			{
				return;
			}

			itemsView.RemoveLogicalChild(View);
		}

		public void Bind(object itemBindingContext, ItemsView itemsView,
			Action<Size> reportMeasure = null, Size? size = null)
		{
			var template = _template.SelectDataTemplate(itemBindingContext, itemsView);

			var templateChanging = template != _selectedTemplate;

			if (templateChanging)
			{
				// Clean up any content we're still holding on to
				_itemContentView.Recycle();

				// Create the new content
				var content = template.CreateContent();
				View = content as View;

				if (View is null)
				{
					throw new InvalidOperationException($"{template} could not be created from {content}");
				}

				// Set the binding context _before_ we create the renderer; that way, the bound data will be 
				// available during OnElementChanged
				View.BindingContext = itemBindingContext;

				// Make sure the Visual property is available when the renderer is created
				PropertyPropagationExtensions.PropagatePropertyChanged(null, View, itemsView);

				// Actually create the native renderer
				_itemContentView.RealizeContent(View, itemsView);

				_selectedTemplate = template;
			}

			_itemContentView.HandleItemSizingStrategy(reportMeasure, size);

			if (!templateChanging)
			{
				// Same template, new data
				View.BindingContext = itemBindingContext;
			}

			itemsView.AddLogicalChild(View);
		}

		protected override bool UseDefaultSelectionColor
		{
			get
			{
				if (View != null)
				{
					return !IsUsingVSMForSelectionColor(View);
				}

				return base.UseDefaultSelectionColor;
			}
		}

		bool IsUsingVSMForSelectionColor(View view)
		{
			var groups = VisualStateManager.GetVisualStateGroups(view);
			for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
			{
				var group = groups[groupIndex];
				for (var stateIndex = 0; stateIndex < group.States.Count; stateIndex++)
				{
					var state = group.States[stateIndex];
					if (state.Name != VisualStateManager.CommonStates.Selected)
					{
						continue;
					}

					for (var setterIndex = 0; setterIndex < state.Setters.Count; setterIndex++)
					{
						var setter = state.Setters[setterIndex];
						if (setter.Property.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
							setter.Property.PropertyName == VisualElement.BackgroundProperty.PropertyName)
						{
							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
