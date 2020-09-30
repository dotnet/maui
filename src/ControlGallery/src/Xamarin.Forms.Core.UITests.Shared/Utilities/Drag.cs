using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal class Drag
	{
		internal enum DragLength
		{
			Long,
			Medium,
			Short
		}

		internal enum Direction
		{
			TopToBottom,
			BottomToTop,
			RightToLeft,
			LeftToRight
		}

		AppRect dragBounds;
		float xStart;
		float yStart;
		float xEnd;
		float yEnd;
		Direction dragDirection;
		Direction oppositeDirection;
		DragLength dragLength;

		public Drag(AppRect dragbounds, float xStart, float yStart, float xEnd, float yEnd, Direction direction)
		{
			dragBounds = dragbounds;
			this.xStart = xStart;
			this.yStart = yStart;
			this.xEnd = xEnd;
			this.yEnd = yEnd;
			dragDirection = direction;
			oppositeDirection = GetOppositeDirection(direction);
		}

		public Drag(AppRect dragBounds, Direction direction, DragLength dragLength)
		{
			this.dragBounds = dragBounds;
			dragDirection = direction;
			this.dragLength = dragLength;
			SetDragForBounds(dragDirection, dragLength);
		}

		void SetDragForBounds(Direction direction, DragLength dragLength)
		{
			// percentage of bounds to scroll centered in element
			float scrollPercentage;

			switch (dragLength)
			{
				case DragLength.Long:
					scrollPercentage = 0.8f;
					break;
				case DragLength.Medium:
					scrollPercentage = 0.5f;
					break;
				default:
					scrollPercentage = 0.2f;
					break;
			}

			if (direction == Direction.LeftToRight)
			{
				yStart = dragBounds.CenterY;
				yEnd = dragBounds.CenterY;
				float xDisplacement = (dragBounds.CenterX + (dragBounds.Width / 2)) - dragBounds.X;
				float insetForScroll = (xDisplacement - (xDisplacement * scrollPercentage)) / 2;
				xStart = dragBounds.X + insetForScroll;
				xEnd = (dragBounds.CenterX + (dragBounds.Width / 2)) - insetForScroll;
			}
			else if (direction == Direction.RightToLeft)
			{
				yStart = dragBounds.CenterY;
				yEnd = dragBounds.CenterY;
				float xDisplacement = (dragBounds.CenterX + (dragBounds.Width / 2)) - dragBounds.X;
				float insetForScroll = (xDisplacement - (xDisplacement * scrollPercentage)) / 2;
				xStart = (dragBounds.CenterX + (dragBounds.Width / 2)) - insetForScroll;
				xEnd = dragBounds.X + insetForScroll;
			}
			else if (direction == Direction.TopToBottom)
			{
				xStart = dragBounds.CenterX;
				xEnd = dragBounds.CenterX;
				float yDisplacement = (dragBounds.CenterY + (dragBounds.Height / 2)) - dragBounds.Y;
				float insetForScroll = (yDisplacement - (yDisplacement * scrollPercentage)) / 2;
				yStart = dragBounds.Y + insetForScroll;
				yEnd = (dragBounds.CenterY + (dragBounds.Height / 2)) - insetForScroll;
			}
			else if (direction == Direction.BottomToTop)
			{
				xStart = dragBounds.CenterX;
				xEnd = dragBounds.CenterX;
				float yDisplacement = (dragBounds.CenterY + (dragBounds.Height / 2)) - dragBounds.Y;
				float insetForScroll = (yDisplacement - (yDisplacement * scrollPercentage)) / 2;
				yStart = (dragBounds.CenterY + (dragBounds.Height / 2)) - insetForScroll;
				yEnd = dragBounds.Y + insetForScroll;

			}
		}

		Direction GetOppositeDirection(Direction direction)
		{
			switch (direction)
			{
				case Direction.TopToBottom:
					return Direction.BottomToTop;
				case Direction.BottomToTop:
					return Direction.TopToBottom;
				case Direction.RightToLeft:
					return Direction.LeftToRight;
				case Direction.LeftToRight:
					return Direction.RightToLeft;
				default:
					return Direction.TopToBottom;
			}
		}

		public AppRect DragBounds
		{
			get { return dragBounds; }
		}

		public float XStart
		{
			get { return xStart; }
		}

		public float YStart
		{
			get { return yStart; }
		}

		public float XEnd
		{
			get { return xEnd; }
		}

		public float YEnd
		{
			get { return yEnd; }
		}

		public Direction DragDirection
		{
			get { return dragDirection; }
			set
			{
				if (dragDirection == value)
					return;

				dragDirection = value;
				oppositeDirection = GetOppositeDirection(dragDirection);
				OnDragDirectionChanged();
			}
		}

		void OnDragDirectionChanged()
		{
			SetDragForBounds(dragDirection, dragLength);
		}

		public Direction OppositeDirection
		{
			get { return oppositeDirection; }
			private set
			{
				if (oppositeDirection == value)
					return;

				oppositeDirection = value;
			}
		}
	}
}