using Gdk;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

public class PlatformTouchGraphicsView : Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView
{
	IGraphicsView? _graphicsView;
	bool _isTouching;
	bool _isInBounds;

	PointF[] GetViewPoints(EventButton e)
	{
		return new[] { new PointF((float)e.X, (float)e.Y) };
	}

	PointF[] GetViewPoints(EventMotion e)
	{
		return new[] { new PointF((float)e.X, (float)e.Y) };
	}

	PointF[] GetViewPoints(EventCrossing e)
	{
		return new[] { new PointF((float)e.X, (float)e.Y) };
	}


	protected override bool OnButtonPressEvent(EventButton e)
	{
		_isTouching = true;
		_graphicsView?.StartInteraction(GetViewPoints(e));
		return base.OnButtonPressEvent(e);
	}

	protected override bool OnButtonReleaseEvent(EventButton e)
	{
		_isTouching = false;
		_graphicsView?.EndInteraction(GetViewPoints(e), _isInBounds);

		return base.OnButtonReleaseEvent(e);
	}

	protected override bool OnTouchEvent(Event evnt)
	{
		_isTouching = true;
		return base.OnTouchEvent(evnt);
	}

	protected override bool OnEnterNotifyEvent(EventCrossing e)
	{
		_isInBounds = true;
		_graphicsView?.StartHoverInteraction(GetViewPoints(e));
		return base.OnEnterNotifyEvent(e);
	}

	protected override bool OnMotionNotifyEvent(EventMotion e)
	{
		_graphicsView?.MoveHoverInteraction(GetViewPoints(e));
		return base.OnMotionNotifyEvent(e);
	}

	protected override bool OnLeaveNotifyEvent(EventCrossing e)
	{
		_isInBounds = false;

		_graphicsView?.EndHoverInteraction();

		if (_isTouching)
		{
			_isTouching = false;
			_graphicsView?.EndInteraction(GetViewPoints(e), _isInBounds);
		}

		return base.OnLeaveNotifyEvent(e);
	}

	public void Connect(IGraphicsView graphicsView)
	{
		_graphicsView = graphicsView;
		Events |= EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.PointerMotionMask |
		          EventMask.ButtonMotionMask | EventMask.LeaveNotifyMask | EventMask.EnterNotifyMask
			;
	}

	public void Disconnect()
	{
		_graphicsView = null;
	}
}