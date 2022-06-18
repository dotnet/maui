using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public abstract class AbstractScenario : IPicture, IDrawable
	{
		public static readonly float[] SOLID = null;
		public static readonly float[] DOT_DOT = {1, 1};
		public static readonly float[] DOTTED = {2, 2};
		public static readonly float[] DASHED = {4, 4};
		public static readonly float[] LONG_DASHES = {8, 4};
		public static readonly float[] EXTRA_LONG_DASHES = {16, 4};
		public static readonly float[] DASHED_DOT = {4, 4, 1, 4};
		public static readonly float[] DASHED_DOT_DOT = {4, 4, 1, 4, 1, 4};
		public static readonly float[] LONG_DASHES_DOT = {8, 4, 2, 4};
		public static readonly float[] EXTRA_LONG_DASHES_DOT = {16, 4, 8, 4};

		private float x;
		private float y;
		private float width;
		private float height;
		private string hash;

		public float X
		{
			get => x;
			set => x = value;
		}

		public float Y
		{
			get => y;
			set => y = value;
		}

		public float Width
		{
			get => width;
			set => width = value;
		}

		public float Height
		{
			get => height;
			set => height = value;
		}

		public AbstractScenario(float x, float y, float width, float height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public AbstractScenario(float width, float height)
		{
			this.width = width;
			this.height = height;
		}

		public virtual void Draw(ICanvas canvas)
		{
			// Do nothing by default
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			Draw(canvas);
		}

		public string Hash
		{
			get => hash;
			set => hash = value;
		}

		public override string ToString()
		{
			return GetType().Name;
		}

		public IImage ToImage(int width, int height, float scale = 1)
		{
			throw new System.NotImplementedException();
		}
	}
}
