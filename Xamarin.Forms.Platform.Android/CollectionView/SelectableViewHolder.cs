using System;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;

namespace Xamarin.Forms.Platform.Android
{
	internal abstract class SelectableViewHolder : RecyclerView.ViewHolder, global::Android.Views.View.IOnClickListener
	{
		bool _isSelected;
		Drawable _selectedDrawable;
		Drawable _selectableItemDrawable;

		protected SelectableViewHolder(global::Android.Views.View itemView) : base(itemView)
		{
			itemView.SetOnClickListener(this);
		}

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;

				if (_isSelected)
				{
					EnsureSelectionStates();
				}

				ItemView.Activated = _isSelected;
				OnSelectedChanged();
			}
		}

		public void OnClick(global::Android.Views.View view)
		{
			OnViewHolderClicked(AdapterPosition);
		}

		public event EventHandler<int> Clicked;

		protected virtual void OnSelectedChanged()
		{
		}

		protected virtual void OnViewHolderClicked(int adapterPosition)
		{
			Clicked?.Invoke(this, adapterPosition);
		}

		void EnsureSelectionStates()
		{
			if (_selectedDrawable != null)
			{
				return;
			}

			if (Forms.IsLollipopOrNewer)
			{
				// We're looking for the foreground ripple effect, which is not available on older APIs
				_selectableItemDrawable = GetSelectableItemDrawable();
				ItemView.Foreground = _selectableItemDrawable;
			}

			_selectedDrawable = GetSelectedDrawable();
				
			if (_selectedDrawable != null)
			{
				ItemView.Background = _selectedDrawable;
			}
		}

		Drawable GetSelectedDrawable()
		{
			using (var value = new TypedValue())
			{
				var context = ItemView.Context;
				
				if(!context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ColorActivatedHighlight, value, true))
				{
					return null;
				}
			
				var color = Color.FromUint((uint)value.Data);
				var colorDrawable = new ColorDrawable(color.ToAndroid());

				var stateListDrawable = new StateListDrawable();
				stateListDrawable.AddState(new[] { global::Android.Resource.Attribute.StateActivated }, colorDrawable);
				stateListDrawable.AddState(StateSet.WildCard.ToArray(), null);

				return stateListDrawable;
			}
		}

		Drawable GetSelectableItemDrawable()
		{
			using (var value = new TypedValue())
			{
				var context = ItemView.Context;

				context.Theme.ResolveAttribute(global::Android.Resource.Attribute.SelectableItemBackground, value, true);

				return ContextCompat.GetDrawable(context, value.ResourceId);
			}
		}
	}
}