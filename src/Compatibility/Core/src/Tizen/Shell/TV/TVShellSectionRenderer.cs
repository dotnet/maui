using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class TVShellSectionRenderer : IShellSectionRenderer, IDisposable
	{
		EBox _mainLayout = null;
		EBox _contentArea = null;

		EvasObject _currentContent = null;
		TVNavigationView _navigationView;

		Dictionary<ShellContent, EvasObject> _contentCache = new Dictionary<ShellContent, EvasObject>();

		bool _disposed = false;
		bool _drawerIsVisible => (ShellSection != null) ? (ShellSection.Items.Count > 1) : false;

		public TVShellSectionRenderer(ShellSection section)
		{
			ShellSection = section;
			ShellSection.PropertyChanged += OnSectionPropertyChanged;
			(ShellSection.Items as INotifyCollectionChanged).CollectionChanged += OnShellSectionCollectionChanged;

			_mainLayout = new EBox(Forms.NativeParent);
			_mainLayout.SetLayoutCallback(OnLayout);

			_contentArea = new EBox(Forms.NativeParent);
			_contentArea.Show();
			_mainLayout.PackEnd(_contentArea);

			UpdateSectionItems();
			UpdateCurrentItem(ShellSection.CurrentItem);
		}

		public ShellSection ShellSection { get; }

		public EvasObject NativeView
		{
			get
			{
				return _mainLayout;
			}
		}

		~TVShellSectionRenderer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (ShellSection != null)
				{
					ShellSection.PropertyChanged -= OnSectionPropertyChanged;
				}

				NativeView.Unrealize();
			}
			_disposed = true;
		}

		void OnNavigationViewSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null)
				return;

			var content = e.SelectedItem;
			if (ShellSection.CurrentItem != content)
			{
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			}
		}

		void OnSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentItem")
			{
				UpdateCurrentItem(ShellSection.CurrentItem);
			}
		}

		void UpdateSectionItems()
		{
			if (!_drawerIsVisible)
			{
				return;
			}

			if (_navigationView == null)
			{
				_navigationView = new TVNavigationView(Forms.NativeParent, ShellSection);
				_navigationView.SetAlignment(-1, -1);
				_navigationView.SetWeight(1, 1);
				_navigationView.Show();
				_mainLayout.PackStart(_navigationView);

				_navigationView.SelectedItemChanged += OnNavigationViewSelectedItemChanged;
			}

			(_navigationView as TVNavigationView).BuildMenu(ShellSection.Items, Shell.GetItemTemplate(ShellSection));
		}

		void UpdateCurrentItem(ShellContent content)
		{
			if (_currentContent != null)
			{
				_currentContent.Hide();
				_contentArea.UnPack(_currentContent);
				_currentContent = null;
			}

			if (content == null)
			{
				return;
			}

			if (!_contentCache.ContainsKey(content))
			{
				var native = CreateShellContent(content);
				native.SetAlignment(-1, -1);
				native.SetWeight(1, 1);
				_contentCache[content] = native;
			}
			_currentContent = _contentCache[content];
			_currentContent.Show();
			_contentArea.PackEnd(_currentContent);
		}

		EvasObject CreateShellContent(ShellContent content)
		{
			Page xpage = ((IShellContentController)content).GetOrCreateContent();
			return Platform.GetOrCreateRenderer(xpage).NativeView;
		}

		void OnLayout()
		{
			if (NativeView.Geometry.Width == 0 || NativeView.Geometry.Height == 0)
				return;

			var bound = NativeView.Geometry;
			var drawerWidth = 0;

			if (_drawerIsVisible && _navigationView != null)
			{
				var drawerBound = bound;
				drawerWidth = _navigationView.GetDrawerWidth();
				drawerBound.Width = drawerWidth;

				_navigationView.Geometry = drawerBound;
			}

			var contentBound = bound;

			contentBound.X += drawerWidth;
			contentBound.Width -= drawerWidth;
			_contentArea.Geometry = contentBound;
		}

		void OnShellSectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateSectionItems();
		}
	}
}
