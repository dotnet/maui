using System;
using System.Collections.Generic;
using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ViewPool : IDisposable
	{
		readonly Dictionary<Type, Stack<AView>> _freeViews = new Dictionary<Type, Stack<AView>>();
		readonly ViewGroup _viewGroup;

		bool _disposed;

		public ViewPool(ViewGroup viewGroup)
		{
			_viewGroup = viewGroup;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			foreach (Stack<AView> views in _freeViews.Values)
			{
				foreach (AView view in views)
					view.Dispose();
			}

			_disposed = true;
		}

		public void ClearChildren()
		{
			if (_disposed)
				throw new ObjectDisposedException(null);

			ClearChildren(_viewGroup);
		}

		public TView GetFreeView<TView>() where TView : AView
		{
			if (_disposed)
				throw new ObjectDisposedException(null);

			Stack<AView> views;
			if (_freeViews.TryGetValue(typeof(TView), out views) && views.Count > 0)
				return (TView)views.Pop();

			return null;
		}

		void ClearChildren(ViewGroup group)
		{
			if (group == null)
				return;

			int count = group.ChildCount;
			for (var i = 0; i < count; i++)
			{
				AView child = group.GetChildAt(i);

				var g = child as ViewGroup;
				if (g != null)
					ClearChildren(g);

				Type childType = child.GetType();
				Stack<AView> stack;
				if (!_freeViews.TryGetValue(childType, out stack))
					_freeViews[childType] = stack = new Stack<AView>();

				stack.Push(child);
			}

			group.RemoveAllViews();
		}
	}
}