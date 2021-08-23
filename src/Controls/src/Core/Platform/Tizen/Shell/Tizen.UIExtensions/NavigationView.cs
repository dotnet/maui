using ElmSharp;
using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;
using EBox = ElmSharp.Box;

namespace Tizen.UIExtensions.Shell
{
	/// <summary>
	/// The native widget that is configured with an header and an list of items to be used in NavigationDrawer.
	/// </summary>
	public class NavigationView : Background, INavigationView
	{
		static readonly EColor s_defaultBackgroundColor = ElmSharp.ThemeConstants.Shell.ColorClass.DefaultNavigationViewBackgroundColor;

		EBox _mainLayout;

		EvasObject _header;
		EvasObject _footer;
		EvasObject _content;

		EvasObject _backgroundImage;
		EColor _backgroundColor;

		/// <summary>
		/// Initializes a new instance of the <see cref="Tizen.UIExtensions.ElmSharp.NavigationView2"/> class.
		/// </summary>
		/// <param name="parent">Parent evas object.</param>
		public NavigationView(EvasObject parent) : base(parent)
		{
			InitializeComponent(parent);
		}

		/// <summary>
		/// Gets or sets the background color of the NavigtiaonView.
		/// </summary>
		public override EColor BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				EColor effectiveColor = _backgroundColor.IsDefault ? s_defaultBackgroundColor : _backgroundColor;
				base.BackgroundColor = effectiveColor;
			}
		}

		/// <summary>
		/// Gets or sets the background image of the NavigtiaonView.
		/// </summary>
		public EvasObject BackgroundImage
		{
			get => _backgroundImage;
			set
			{
				_backgroundImage = value;
				this.SetBackgroundPart(_backgroundImage);
			}
		}

		/// <summary>
		/// Gets or sets the header view of the NavigtiaonView.
		/// </summary>
		public EvasObject Header
		{
			get => _header;
			set => UpdateHeader(value);
		}

		/// <summary>
		/// Gets or sets the footer view of the NavigtiaonView.
		/// </summary>
		public EvasObject Footer
		{
			get => _footer;
			set => UpdateFooter(value);
		}

		public EvasObject Content
		{
			get => _content;
			set => UpdateContent(value);
		}

		/// <summary>
		/// Gets or sets the target view of the NavigtiaonView.
		/// </summary>
		public EvasObject TargetView => this;

		/// <summary>
		/// Notifies that the layout has been updated.
		/// </summary>
		public event EventHandler<LayoutEventArgs> LayoutUpdated;

		void InitializeComponent(EvasObject parent)
		{
			base.BackgroundColor = s_defaultBackgroundColor;

			_mainLayout = new EBox(parent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1
			};
			_mainLayout.SetLayoutCallback(OnLayout);
			_mainLayout.Show();

			SetContent(_mainLayout);
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			var bound = Geometry;
			int headerHeight = 0;
			int footerHeight = 0;

			if (_header != null)
			{
				var headerBound = bound;
				headerHeight = _header.MinimumHeight;
				headerBound.Height = headerHeight;
				_header.Geometry = headerBound;
			}

			if (_footer != null)
			{
				var footerbound = bound;
				footerHeight = _footer.MinimumHeight;
				footerbound.Y = bound.Y + bound.Height - footerHeight;
				footerbound.Height = footerHeight;
				_footer.Geometry = footerbound;
			}

			if (_content != null)
			{
				bound.Y += headerHeight;
				bound.Height = bound.Height - headerHeight - footerHeight;
				_content.Geometry = bound;
			}

			NotifyOnLayout();
		}

		void NotifyOnLayout()
		{
			LayoutUpdated?.Invoke(this, new LayoutEventArgs() { Geometry = Geometry.ToCommon() });
		}

		void UpdateHeader(EvasObject header)
		{
			if (_header != null)
			{
				_mainLayout.UnPack(_header);
				_header.Unrealize();
				_header = null;
			}

			if (header != null)
			{
				_mainLayout.PackStart(header);
			}
			_header = header;
			_header?.Show();
		}

		void UpdateFooter(EvasObject footer)
		{
			if (_footer != null)
			{
				_mainLayout.UnPack(_footer);
				_footer.Unrealize();
				_footer = null;
			}

			if (footer != null)
			{
				_mainLayout.PackEnd(footer);
			}
			_footer = footer;
			_footer?.Show();
		}

		void UpdateContent(EvasObject content)
		{
			if (_content != null)
			{
				_mainLayout.UnPack(_content);
				_content.Unrealize();
				_content = null;
			}

			if (content != null)
			{
				_mainLayout.PackEnd(content);
			}
			_content = content;
			_content?.Show();
		}
	}
}
