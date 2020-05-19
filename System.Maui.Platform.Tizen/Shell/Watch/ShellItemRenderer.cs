using System.Collections.Generic;
using System.ComponentModel;
using ElmSharp;

namespace System.Maui.Platform.Tizen.Watch
{
	public class ShellItemRenderer : IShellItemRenderer
	{
		Box _mainLayout;
		EvasObject _currentItem;
		Dictionary<BaseShellItem, IShellItemRenderer> _rendererCache = new Dictionary<BaseShellItem, IShellItemRenderer>();

		public ShellItemRenderer(ShellItem item)
		{
			ShellItem = item;
			ShellItem.PropertyChanged += OnItemPropertyChanged;
			InitializeComponent();
			UpdateCurrentItem();
		}

		public ShellItem ShellItem { get; protected set; }

		public BaseShellItem Item => ShellItem;

		public EvasObject NativeView => _mainLayout;

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				ResetCurrentItem();
				ShellItem.PropertyChanged -= OnItemPropertyChanged;
			}
		}

		void InitializeComponent()
		{
			_mainLayout = new Box(System.Maui.Maui.NativeParent);
			_mainLayout.SetLayoutCallback(OnLayout);
		}

		void UpdateCurrentItem()
		{
			ResetCurrentItem();
			var currentItem = ShellItem.CurrentItem;
			if (currentItem != null)
			{
				if (!_rendererCache.TryGetValue(currentItem, out IShellItemRenderer renderer))
				{
					renderer = ShellRendererFactory.Default.CreateShellNavigationRenderer(currentItem);
					_rendererCache[currentItem] = renderer;
				}
				SetCurrentItem(renderer.NativeView);
			}
		}

		void SetCurrentItem(EvasObject item)
		{
			_currentItem = item;
			_currentItem.Show();
			_mainLayout.PackEnd(_currentItem);
		}

		void ResetCurrentItem()
		{
			if (_currentItem != null)
			{
				_mainLayout.UnPack(_currentItem);
				_currentItem.Hide();
				_currentItem = null;
			}
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShellItem.CurrentItem))
			{
				UpdateCurrentItem();
			}
		}

		void OnLayout()
		{
			if (_currentItem != null)
			{
				_currentItem.Geometry = _mainLayout.Geometry;
			}
		}
	}
}
