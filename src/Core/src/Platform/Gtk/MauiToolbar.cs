using Gtk;

namespace Microsoft.Maui.Platform
{
	public class MauiToolbar : Box
	{
		internal Button BackButton { get; }

		Label? _titleLabel;

		internal Label TitleLabel
		{
			get
			{
				if (_titleLabel != null)
				{
					return _titleLabel;
				}

				_titleLabel = new Label();
				PackStart(_titleLabel, false, false, 0);
				_titleLabel.Show();
				return _titleLabel;
			}
		}

		void RemoveTitleLabel()
		{
			if (_titleLabel is not { })
			{
				return;
			}

			Remove(_titleLabel);
			QueueResize();
			_titleLabel = default;
		}

		string _title = string.Empty;

		public string Title
		{
			get => _title;
			set
			{
				if (_title == value)
					return;

				_title = value;
				if (_title != string.Empty)
				{
					TitleLabel.Text = _title;
					QueueResize();
				}
				else
				{
					RemoveTitleLabel();
				}
			}
		}

		public delegate void BackButtonClickedEventHandler(object? sender);

		public event BackButtonClickedEventHandler? BackButtonClicked;

		public MauiToolbar() : base(Orientation.Horizontal, 0)
		{
			BackButton = new Button();
			BackButton.Image = new Image(Stock.GoBack, IconSize.Button);
			// Remove button border
			BackButton.Relief = ReliefStyle.None;

			BackButton.Clicked += (sender, args) => BackButtonClicked?.Invoke(sender);

			PackStart(BackButton, false, false, 0);
			NoShowAll = true;
		}
	}
}