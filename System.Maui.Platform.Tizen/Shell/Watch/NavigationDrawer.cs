using System;
using System.Threading;
using System.Threading.Tasks;
using ElmSharp;
using ElmSharp.Wearable;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using EImage = ElmSharp.Image;
using ELayout = ElmSharp.Layout;
using EWidget = ElmSharp.Widget;

namespace Xamarin.Forms.Platform.Tizen.Watch
{
	public class NavigationDrawer : ELayout, IAnimatable
	{
		static readonly int TouchWidth = 50;
		static readonly int IconSize = 40;
		static readonly string DefaultIcon = "Xamarin.Forms.Platform.Tizen.Resource.wc_visual_cue.png";

		Box _mainLayout;
		Box _contentGestureBox;
		Box _contentBox;
		Box _drawerBox;
		Box _drawerContentBox;
		Box _drawerIconBox;

		EvasObject _content;
		EvasObject _drawerContent;

		EImage _drawerIcon;
		EButton _touchArea;

		GestureLayer _gestureOnContent;
		GestureLayer _gestureOnDrawer;

		ImageSource _drawerIconSource;

		bool _isOpen;
		bool _isDefaultIcon;

		CancellationTokenSource _fadeInCancelTokenSource = null;

		bool HasDrawer => _drawerBox != null;

		public NavigationDrawer(EvasObject parent) : base(parent)
		{
			Initialize();
		}

		public int HandlerHeight { get; set; } = 40;

		public bool IsOpen
		{
			get
			{
				return _isOpen;
			}
			set
			{
				if (_isOpen != value)
				{
					if (value)
					{
						Open();
					}
					else
					{
						Close();
					}
				}
			}
		}

		EColor _handlerBackgroundColor = EColor.Transparent;
		public EColor HandlerBackgroundColor
		{
			get => _handlerBackgroundColor;
			set
			{
				_handlerBackgroundColor = value;
				UpdateHandlerBackgroundColor();
			}
		}

		public event EventHandler Toggled;

		public void SetMainContent(EvasObject content)
		{
			if (content == null)
			{
				UnsetMainContent();
				return;
			}

			_content = content;
			_content.Show();
			_contentBox.PackEnd(_content);
			_content.Geometry = _contentBox.Geometry;
		}

		public void SetDrawerContent(EvasObject content)
		{
			InitializeDrawerBox();

			if (content == null)
			{
				UnsetDrawerContent();
				return;
			}

			_drawerContent = content;
			_drawerContent.Show();
			_drawerContentBox.PackEnd(_drawerContent);

			_drawerContentBox.Show();
			_drawerIconBox.Show();

			if (_drawerContent is NavigationView nv)
			{
				nv.Dragged += (s, e) =>
				{
					if (e.State == DraggedState.EdgeTop)
					{
						Close();
					}
				};
			}
		}

		public void UpdateDrawerIcon(ImageSource source)
		{
			_drawerIconSource = source;
			if (HasDrawer)
			{
				SetDrawerIcon(_drawerIconSource);
			}
		}

		public async void Open(uint length = 300)
		{
			if (!HasDrawer)
				return;

			var toMove = _drawerBox.Geometry;
			toMove.Y = 0;

			await RunMoveAnimation(_drawerBox, toMove, length);

			if (!_isOpen)
			{
				_isOpen = true;
				Toggled?.Invoke(this, EventArgs.Empty);
			}
			OnLayout();
			OnDrawerLayout();
			FlipIcon();
		}

		public async void Close(uint length = 300)
		{
			if (!HasDrawer)
				return;

			var toMove = _drawerBox.Geometry;
			toMove.Y = Geometry.Height - HandlerHeight;

			await RunMoveAnimation(_drawerBox, toMove, length);

			if (_isOpen)
			{
				_isOpen = false;
				Toggled?.Invoke(this, EventArgs.Empty);
			}
			OnLayout();
			OnDrawerLayout();
			ResetIcon();
			StartHighlightAnimation(_drawerIcon);
		}

		void IAnimatable.BatchBegin()
		{
		}

		void IAnimatable.BatchCommit()
		{
		}

		protected override IntPtr CreateHandle(EvasObject parent)
		{
			_mainLayout = new Box(parent);
			return _mainLayout.Handle;
		}

		void Initialize()
		{
			_mainLayout.SetLayoutCallback(OnLayout);

			_contentGestureBox = new Box(_mainLayout);
			_contentGestureBox.Show();
			_mainLayout.PackEnd(_contentGestureBox);

			_contentBox = new Box(_mainLayout);
			_contentBox.SetLayoutCallback(OnContentLayout);
			_contentBox.Show();
			_mainLayout.PackEnd(_contentBox);
		}

		void InitializeDrawerBox()
		{
			if (_drawerBox != null)
				return;

			_drawerBox = new Box(_mainLayout);
			_drawerBox.SetLayoutCallback(OnDrawerLayout);
			_drawerBox.Show();
			_mainLayout.PackEnd(_drawerBox);

			_drawerContentBox = new Box(_drawerBox);
			_drawerBox.PackEnd(_drawerContentBox);

			_drawerIconBox = new Box(_drawerBox)
			{
				BackgroundColor = _handlerBackgroundColor
			};
			_drawerBox.PackEnd(_drawerIconBox);

			_drawerIcon = new EImage(_drawerIconBox)
			{
				AlignmentY = 0.5,
				AlignmentX = 0.5,
				MinimumHeight = IconSize,
				MinimumWidth = IconSize,
			};
			_drawerIcon.Show();
			_drawerIconBox.PackEnd(_drawerIcon);
			SetDrawerIcon(_drawerIconSource);

			_touchArea = new EButton(_drawerBox)
			{
				Color = EColor.Transparent,
				BackgroundColor = EColor.Transparent,
			};
			_touchArea.SetPartColor("effect", EColor.Transparent);
			_touchArea.Show();
			_touchArea.RepeatEvents = true;
			_touchArea.Clicked += OnIconClicked;

			_drawerBox.PackEnd(_touchArea);

			_gestureOnContent = new GestureLayer(_contentGestureBox);
			_gestureOnContent.SetMomentumCallback(GestureLayer.GestureState.Start, OnContentDragStarted);
			_gestureOnContent.SetMomentumCallback(GestureLayer.GestureState.End, OnContentDragEnded);
			_gestureOnContent.SetMomentumCallback(GestureLayer.GestureState.Abort, OnContentDragEnded);
			_gestureOnContent.Attach(_contentGestureBox);
			_contentBox.RepeatEvents = true;

			_gestureOnDrawer = new GestureLayer(_drawerIconBox);
			_gestureOnDrawer.SetMomentumCallback(GestureLayer.GestureState.Move, OnDrawerDragged);
			_gestureOnDrawer.SetMomentumCallback(GestureLayer.GestureState.End, OnDrawerDragEnded);
			_gestureOnDrawer.SetMomentumCallback(GestureLayer.GestureState.Abort, OnDrawerDragEnded);
			_gestureOnDrawer.Attach(_drawerIconBox);

			RotaryEventManager.Rotated += OnRotateEventReceived;
		}

		void SetDrawerIcon(ImageSource source)
		{
			if (source == null)
			{
				_ = _drawerIcon.LoadFromImageSourceAsync(ImageSource.FromResource(DefaultIcon, GetType().Assembly));
				_isDefaultIcon = true;
			}
			else
			{
				_isDefaultIcon = false;
				if (source is FileImageSource fsource)
				{
					_drawerIcon.Load(fsource.ToAbsPath());
				}
				else
				{
					_ = _drawerIcon.LoadFromImageSourceAsync(source);
				}
			}
		}

		void UpdateHandlerBackgroundColor()
		{
			if (_drawerIconBox != null)
			{
				_drawerIconBox.BackgroundColor = _handlerBackgroundColor;
			}
		}

		void OnIconClicked(object sender, EventArgs e)
		{
			if (IsOpen)
				Close();
			else
				Open();
		}

		async Task<bool> ShowAsync(EWidget target, Easing easing = null, uint length = 300, CancellationToken cancelltaionToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<bool>();

			await Task.Delay(1000);

			if (cancelltaionToken.IsCancellationRequested)
			{
				cancelltaionToken.ThrowIfCancellationRequested();
			}

			target.Show();
			var opacity = target.Opacity;

			if (opacity == 255 || opacity == -1)
				return true;

			new Animation((progress) =>
			{
				target.Opacity = opacity + (int)((255 - opacity) * progress);

			}).Commit(this, "FadeIn", length: length, finished: (p, e) =>
			{
				target.Opacity = 255;
				tcs.SetResult(true);
				StartHighlightAnimation(_drawerIcon);
			});

			return await tcs.Task;
		}

		void OnLayout()
		{
			var bound = Geometry;
			_contentGestureBox.Geometry = bound;
			_contentBox.Geometry = bound;
			if (_drawerBox != null)
			{
				bound.Y = _isOpen ? 0 : (bound.Height - HandlerHeight);
				_drawerBox.Geometry = bound;
			}
		}

		void OnContentLayout()
		{
			if (_content != null)
			{
				_content.Geometry = _contentBox.Geometry;
			}
		}

		void OnDrawerLayout()
		{
			this.AbortAnimation("HighlightAnimation");

			var bound = _drawerBox.Geometry;

			var currentY = bound.Y;
			var ratio = currentY / (double)(Geometry.Height - HandlerHeight);

			var contentBound = bound;
			contentBound.Y += (int)(HandlerHeight * ratio);
			_drawerContentBox.Geometry = contentBound;

			var drawerHandleBound = bound;
			drawerHandleBound.Height = HandlerHeight;
			_drawerIconBox.Geometry = drawerHandleBound;

			var drawerTouchBound = drawerHandleBound;
			drawerTouchBound.Width = TouchWidth;
			drawerTouchBound.X = drawerHandleBound.X + (drawerHandleBound.Width - TouchWidth) / 2;
			_touchArea.Geometry = drawerTouchBound;
		}

		async Task<bool> HideAsync(EWidget target, Easing easing = null, uint length = 300)
		{
			var tcs = new TaskCompletionSource<bool>();

			var opacity = target.Opacity;
			if (opacity == -1)
				opacity = 255;

			new Animation((progress) =>
			{
				target.Opacity = opacity - (int)(progress * opacity);

			}).Commit(this, "FadeOut", length: length, finished: (p, e) =>
			{
				target.Opacity = 0;
				target.Hide();
				tcs.SetResult(true);
			});

			return await tcs.Task;
		}

		void StartHighlightAnimation(EWidget target)
		{
			if (!_isDefaultIcon || this.AnimationIsRunning("HighlightAnimation"))
				return;

			int count = 2;
			var bound = target.Geometry;
			var y = bound.Y;
			var dy = bound.Y - bound.Height / 3;

			var anim = new Animation();

			var transfAnim = new Animation((f) =>
			{
				bound.Y = (int)f;
				var map = new EvasMap(4);
				map.PopulatePoints(bound, 0);
				target.IsMapEnabled = true;
				target.EvasMap = map;
			}, y, dy);

			var opacityAnim = new Animation(f => target.Opacity = (int)f, 255, 40);

			anim.Add(0, 1, opacityAnim);
			anim.Add(0, 1, transfAnim);

			anim.Commit(this, "HighlightAnimation", 16, 800, finished: (f, b) =>
			{
				target.Opacity = 255;
				target.IsMapEnabled = false;
			}, repeat: () => --count > 0);
		}

		async void OnRotateEventReceived(EventArgs args)
		{
			_fadeInCancelTokenSource?.Cancel();
			_fadeInCancelTokenSource = new CancellationTokenSource();

			if (!_isOpen)
			{
				var token = _fadeInCancelTokenSource.Token;
				await HideAsync(_drawerBox);
				_ = ShowAsync(_drawerBox, cancelltaionToken: token);
			}
		}

		void OnContentDragStarted(GestureLayer.MomentumData moment)
		{
			_fadeInCancelTokenSource?.Cancel();
			_fadeInCancelTokenSource = null;

			if (!_isOpen)
			{
				_ = HideAsync(_drawerBox);
			}
		}

		void OnContentDragEnded(GestureLayer.MomentumData moment)
		{
			_fadeInCancelTokenSource = new CancellationTokenSource();
			_ = ShowAsync(_drawerBox, cancelltaionToken: _fadeInCancelTokenSource.Token);
		}

		void OnDrawerDragged(GestureLayer.MomentumData moment)
		{
			var toMove = _drawerBox.Geometry;
			toMove.Y = (moment.Y2 < 0) ? 0 : moment.Y2;
			_drawerBox.Geometry = toMove;
			OnDrawerLayout();
		}

		void OnDrawerDragEnded(GestureLayer.MomentumData moment)
		{
			if (_drawerBox.Geometry.Y < (_mainLayout.Geometry.Height / 2))
			{
				Open();
			}
			else
			{
				Close();
			}
		}

		void FlipIcon()
		{
			if (_isDefaultIcon)
			{
				_drawerIcon.Orientation = ImageOrientation.FlipVertical;
			}
		}

		void ResetIcon()
		{
			_drawerIcon.Orientation = ImageOrientation.None;
		}

		Task RunMoveAnimation(EvasObject target, Rect dest, uint length, Easing easing = null)
		{
			var tcs = new TaskCompletionSource<bool>();

			var dx = target.Geometry.X - dest.X;
			var dy = target.Geometry.Y - dest.Y;

			new Animation((progress) =>
			{
				var toMove = dest;
				toMove.X += (int)(dx * (1 - progress));
				toMove.Y += (int)(dy * (1 - progress));
				target.Geometry = toMove;
				OnDrawerLayout();
			}).Commit(this, "Move", length: length, finished: (s, e) =>
			{
				target.Geometry = dest;
				tcs.SetResult(true);
			});
			return tcs.Task;
		}

		void UnsetMainContent()
		{
			if (_content != null)
			{
				_contentBox.UnPack(_content);
				_content.Hide();
				_content = null;
			}
		}

		void UnsetDrawerContent()
		{
			if (_drawerContent != null)
			{
				_drawerContentBox.UnPack(_drawerContent);
				_drawerContent.Hide();
				_drawerContent = null;

				_drawerContentBox.Hide();
				_drawerIconBox.Hide();
			}
		}
	}
}