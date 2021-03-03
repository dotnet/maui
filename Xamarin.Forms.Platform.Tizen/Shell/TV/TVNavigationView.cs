using System;
using System.Collections.Generic;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;
using Xamarin.Forms.Platform.Tizen.TV;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using EImage = ElmSharp.Image;
using NCollectionView = Xamarin.Forms.Platform.Tizen.Native.CollectionView;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TVNavigationView : Background, INavigationView
	{
		static EColor s_defaultBackgroundColor = EColor.Black;

		EBox _mainLayout;

		EImage _backgroundImage;
		Aspect _bgImageAspect;
		ImageSource _bgImageSource;

		View _header;
		EvasObject _nativeHeader;

		NCollectionView _list;

		EColor _backgroundColor;

		List<List<Element>> _cachedGroups;

		IEnumerable<Element> _cacheditems;

		public TVNavigationView(EvasObject parent) : base(parent)
		{
			InitializeComponent(parent);
		}

		public TVNavigationView(EvasObject parent, Element element) : this(parent)
		{
			Element = element;
		}

		public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;

		public event EventHandler ContentFocused;

		public event EventHandler ContentUnfocused;

		public EvasObject NativeView => this;

		public Element Element { get; }

		FlyoutHeaderBehavior _headerBehavior = FlyoutHeaderBehavior.Fixed;

		public View Header
		{
			get
			{
				return _header;
			}
			set
			{
				UpdateHeader(value);
			}
		}

		public FlyoutHeaderBehavior HeaderBehavior
		{
			get => _headerBehavior;
			set
			{
				if (_headerBehavior == value)
					return;
				_headerBehavior = value;
				UpdateHeaderBehavior();
			}
		}

		public override EColor BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				EColor effectiveColor = _backgroundColor.IsDefault ? s_defaultBackgroundColor : _backgroundColor;
				base.BackgroundColor = effectiveColor;
			}
		}

		public Aspect BackgroundImageAspect
		{
			get
			{
				return _bgImageAspect;
			}
			set
			{
				_bgImageAspect = value;
				_backgroundImage?.ApplyAspect(_bgImageAspect);
			}
		}

		public ImageSource BackgroundImageSource
		{
			get
			{
				return _bgImageSource;
			}
			set
			{
				_bgImageSource = value;
				UpdateBackgroundImage();
			}
		}

		bool HeaderOnMenu => _headerBehavior == FlyoutHeaderBehavior.Scroll ||
							 _headerBehavior == FlyoutHeaderBehavior.CollapseOnScroll;

		public void BuildMenu(List<List<Element>> flyoutGroups)
		{
			if (!IsMenuItemChanged(flyoutGroups))
			{
				return;
			}
			_cachedGroups = flyoutGroups;

			var items = new List<Element>();
			foreach (var group in flyoutGroups)
			{
				foreach (var element in group)
				{
					items.Add(element);
				}
			}

			BuildMenu(items);
		}

		public void BuildMenu(IEnumerable<Element> items, DataTemplate itemTemplate = null)
		{
			var datatemplate = new FlyoutItemTemplateSelector(this, itemTemplate, null);
			if (Element is Shell shell)
			{
				datatemplate = new FlyoutItemTemplateSelector(this, shell.ItemTemplate, shell.MenuItemTemplate);
			}

			_cacheditems = items;
			_list.Adaptor = new FlyoutItemTemplateAdaptor(Element, items, datatemplate, HeaderOnMenu);
			_list.Adaptor.ItemSelected += OnItemSelected;
		}

		public int GetDrawerWidth()
		{
			if (_list is ICollectionViewController controller)
				return controller.GetItemSize(Forms.NativeParent.Geometry.Width / 2, Forms.NativeParent.Geometry.Height).Width;
			else
				return 0;
		}

		void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs(e.SelectedItem, -1));
		}

		void InitializeComponent(EvasObject parent)
		{
			base.BackgroundColor = s_defaultBackgroundColor;

			_mainLayout = new EBox(parent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			_mainLayout.SetLayoutCallback(OnLayout);
			_mainLayout.Show();

			SetContent(_mainLayout);
			CreateMenu();
		}

		void CreateMenu()
		{
			_list = new NCollectionView(Forms.NativeParent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				SelectionMode = Native.CollectionViewSelectionMode.Single,
				HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
				VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
				LayoutManager = new Native.LinearLayoutManager(false, ItemSizingStrategy.MeasureFirstItem)
			};

			_list.Show();
			_mainLayout.PackEnd(_list);

			_list.Focused += OnListFocused;
			_list.Unfocused += OnListUnfocused;

		}

		void OnListFocused(object sender, EventArgs args)
		{
			ContentFocused?.Invoke(this, EventArgs.Empty);
		}

		void OnListUnfocused(object sender, EventArgs args)
		{
			ContentUnfocused?.Invoke(this, EventArgs.Empty);
		}

		bool IsMenuItemChanged(List<List<Element>> flyoutGroups)
		{
			if (_cachedGroups == null)
				return true;

			if (_cachedGroups.Count != flyoutGroups.Count)
				return true;

			for (int i = 0; i < flyoutGroups.Count; i++)
			{
				if (_cachedGroups[i].Count != flyoutGroups[i].Count)
					return true;

				for (int j = 0; j < flyoutGroups[i].Count; j++)
				{
					if (_cachedGroups[i][j] != flyoutGroups[i][j])
						return true;
				}
			}
			return false;
		}

		void UpdateBackgroundImage()
		{
			if (BackgroundImageSource == null)
			{
				if (_backgroundImage != null)
				{
					this.SetBackgroundPart(null);
					_backgroundImage = null;
				}
			}
			else
			{
				if (_backgroundImage == null)
				{
					_backgroundImage = new EImage(this);
					this.SetBackgroundPart(_backgroundImage);
				}
				_backgroundImage.LoadFromImageSourceAsync(BackgroundImageSource).GetAwaiter().OnCompleted(() =>
				{
					_backgroundImage.ApplyAspect(_bgImageAspect);
				});
			}
		}

		void UpdateHeader(View header)
		{
			if (_header != null)
			{
				_header.MeasureInvalidated -= OnHeaderSizeChanged;

				if (HeaderOnMenu)
				{
					ResetHeader();
				}

				if (_nativeHeader != null)
				{
					_mainLayout.UnPack(_nativeHeader);
					_nativeHeader.Unrealize();
					_nativeHeader = null;
				}
			}

			if (header != null)
			{
				header.MeasureInvalidated += OnHeaderSizeChanged;

				if (HeaderOnMenu)
				{
					UpdateHeaderOnMenu();
				}
				else
				{
					var renderer = Platform.GetOrCreateRenderer(header);
					(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
					_nativeHeader = renderer.NativeView;
					_mainLayout.PackEnd(_nativeHeader);
				}
			}

			_header = header;
		}

		void UpdateHeaderBehavior()
		{
			if (_header == null)
				return;

			if (HeaderOnMenu)
			{
				if (_nativeHeader != null)
				{
					_mainLayout.UnPack(_nativeHeader);
					_nativeHeader.Unrealize();
					_nativeHeader = null;
				}
			}
			else
			{
				ResetHeader();
				if (_nativeHeader == null)
				{
					var renderer = Platform.GetOrCreateRenderer(_header);
					(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();
					_nativeHeader = renderer.NativeView;
					_mainLayout.PackEnd(_nativeHeader);
				}
			}
			UpdateHeaderOnMenu();
			OnLayout();
		}

		void UpdateHeaderOnMenu()
		{
			if (_list.Adaptor != null)
				_list.Adaptor.ItemSelected -= OnItemSelected;

			BuildMenu(_cacheditems);
		}

		void ResetHeader()
		{
			if (_header != null)
			{
				Platform.GetRenderer(_header)?.Dispose();
				_nativeHeader = null;
			}
		}

		void OnHeaderSizeChanged(object sender, EventArgs e)
		{
			Device.BeginInvokeOnMainThread(OnLayout);
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			var bound = Geometry;
			int headerHeight = 0;
			if (Header != null)
			{
				if (!HeaderOnMenu)
				{
					var requestSize = Header.Measure(Forms.ConvertToScaledDP(bound.Width), Forms.ConvertToScaledDP(bound.Height));
					headerHeight = Forms.ConvertToScaledPixel(requestSize.Request.Height);
					var headerBound = Geometry;
					headerBound.Height = headerHeight;
					_nativeHeader.Geometry = headerBound;
				}
			}
			bound.Y += headerHeight;
			bound.Height -= headerHeight;

			_list.Geometry = bound;
		}
	}
}
