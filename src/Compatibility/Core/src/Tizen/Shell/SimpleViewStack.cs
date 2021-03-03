using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class SimpleViewStack : EBox
	{
		EvasObject _lastTop;

		public SimpleViewStack(EvasObject parent) : base(parent)
		{
			InternalStack = new List<EvasObject>();
			SetLayoutCallback(OnLayout);
		}

		List<EvasObject> InternalStack { get; set; }

		public IReadOnlyList<EvasObject> Stack => InternalStack;

		public void Push(EvasObject view)
		{
			InternalStack.Add(view);
			PackEnd(view);
			UpdateTopView();
		}

		public void Pop()
		{
			if (_lastTop != null)
			{
				var tobeRemoved = _lastTop;
				InternalStack.Remove(tobeRemoved);
				UnPack(tobeRemoved);
				UpdateTopView();
				// if Pop was called by removed page,
				// Unrealize cause deletation of NativeCallback, it could be a cause of crash
				Device.BeginInvokeOnMainThread(() =>
				{
					tobeRemoved.Unrealize();
				});
			}
		}

		public void PopToRoot()
		{
			while (InternalStack.Count > 1)
			{
				Pop();
			}
		}

		public void Insert(EvasObject before, EvasObject view)
		{
			view.Hide();
			var idx = InternalStack.IndexOf(before);
			InternalStack.Insert(idx, view);
			PackEnd(view);
			UpdateTopView();
		}

		public void Remove(EvasObject view)
		{
			InternalStack.Remove(view);
			UnPack(view);
			UpdateTopView();
			Device.BeginInvokeOnMainThread(() =>
			{
				view?.Unrealize();
			});
		}

		void UpdateTopView()
		{
			if (_lastTop != InternalStack.LastOrDefault())
			{
				_lastTop?.Hide();
				_lastTop = InternalStack.LastOrDefault();
				_lastTop.Show();
				(_lastTop as Widget)?.SetFocus(true);
			}
		}

		void OnLayout()
		{
			foreach (var view in Stack)
			{
				view.Geometry = Geometry;
			}
		}
	}
}
