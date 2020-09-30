using System;

namespace Xamarin.Forms.Shapes
{
	internal enum MatrixTypes
	{
		Identity = 0,
		Translation = 1,
		Scaling = 2,
		Unknown = 4
	}

	[TypeConverter(typeof(MatrixTypeConverter))]
	public struct Matrix
	{
		internal double _m11;
		internal double _m12;
		internal double _m21;
		internal double _m22;
		internal double _offsetX;
		internal double _offsetY;
		internal MatrixTypes _type;
		internal int _padding;

		static Matrix IdentityMatrix = CreateIdentity();

		public Matrix(double m11, double m12,
					  double m21, double m22,
					  double offsetX, double offsetY)
		{
			_m11 = m11;
			_m12 = m12;
			_m21 = m21;
			_m22 = m22;
			_offsetX = offsetX;
			_offsetY = offsetY;
			_type = MatrixTypes.Unknown;
			_padding = 0;

			DeriveMatrixType();
		}

		public static Matrix Identity { get { return IdentityMatrix; } }

		public void SetIdentity()
		{
			_type = MatrixTypes.Identity;
		}

		public bool IsIdentity
		{
			get
			{
				return _type == MatrixTypes.Identity ||
					(_m11 == 1 && _m12 == 0 && _m21 == 0 && _m22 == 1 && _offsetX == 0 && _offsetY == 0);
			}
		}

		public static Matrix operator *(Matrix trans1, Matrix trans2)
		{
			MatrixUtil.MultiplyMatrix(ref trans1, ref trans2);
			return trans1;
		}

		public static Matrix Multiply(Matrix trans1, Matrix trans2)
		{
			MatrixUtil.MultiplyMatrix(ref trans1, ref trans2);
			return trans1;
		}

		public void Append(Matrix matrix)
		{
			this *= matrix;
		}

		public void Prepend(Matrix matrix)
		{
			this = matrix * this;
		}

		public void Rotate(double angle)
		{
			angle %= 360.0;
			this *= CreateRotationRadians(angle * (Math.PI / 180.0));
		}

		public void RotatePrepend(double angle)
		{
			angle %= 360.0;
			this = CreateRotationRadians(angle * (Math.PI / 180.0)) * this;
		}

		public void RotateAt(double angle, double centerX, double centerY)
		{
			angle %= 360.0;
			this *= CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY);
		}

		public void RotateAtPrepend(double angle, double centerX, double centerY)
		{
			angle %= 360.0;
			this = CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY) * this;
		}

		public void Scale(double scaleX, double scaleY)
		{
			this *= CreateScaling(scaleX, scaleY);
		}

		public void ScalePrepend(double scaleX, double scaleY)
		{
			this = CreateScaling(scaleX, scaleY) * this;
		}

		public void ScaleAt(double scaleX, double scaleY, double centerX, double centerY)
		{
			this *= CreateScaling(scaleX, scaleY, centerX, centerY);
		}

		public void ScaleAtPrepend(double scaleX, double scaleY, double centerX, double centerY)
		{
			this = CreateScaling(scaleX, scaleY, centerX, centerY) * this;
		}

		public void Skew(double skewX, double skewY)
		{
			skewX %= 360;
			skewY %= 360;
			this *= CreateSkewRadians(skewX * (Math.PI / 180.0),
									  skewY * (Math.PI / 180.0));
		}

		public void SkewPrepend(double skewX, double skewY)
		{
			skewX %= 360;
			skewY %= 360;
			this = CreateSkewRadians(skewX * (Math.PI / 180.0),
									 skewY * (Math.PI / 180.0)) * this;
		}

		public void Translate(double offsetX, double offsetY)
		{
			//
			// / a b 0 \   / 1 0 0 \    / a      b       0 \
			// | c d 0 | * | 0 1 0 | = |  c      d       0 |
			// \ e f 1 /   \ x y 1 /    \ e+x    f+y     1 /
			//
			// (where e = _offsetX and f == _offsetY)
			//

			if (_type == MatrixTypes.Identity)
			{
				SetMatrix(1, 0,
						  0, 1,
						  offsetX, offsetY,
						  MatrixTypes.Translation);
			}
			else if (_type == MatrixTypes.Unknown)
			{
				_offsetX += offsetX;
				_offsetY += offsetY;
			}
			else
			{
				_offsetX += offsetX;
				_offsetY += offsetY;

				_type |= MatrixTypes.Translation;
			}
		}

		public void TranslatePrepend(double offsetX, double offsetY)
		{
			this = CreateTranslation(offsetX, offsetY) * this;
		}

		public Point Transform(Point point)
		{
			Point newPoint = point;

			double x = newPoint.X;
			double y = newPoint.Y;

			MultiplyPoint(ref x, ref y);

			return new Point(x, y);
		}

		public void Transform(Point[] points)
		{
			if (points != null)
			{
				for (int i = 0; i < points.Length; i++)
				{
					var point = points[i];
					double x = point.X;
					double y = point.Y;

					MultiplyPoint(ref x, ref y);

					points[i] = new Point(x, y);
				}
			}
		}

		public Vector2 Transform(Vector2 vector)
		{
			Vector2 newVector = vector;

			double x = newVector.X;
			double y = newVector.Y;

			MultiplyVector(ref x, ref y);

			return new Vector2(x, y);
		}

		public void Transform(Vector2[] vectors)
		{
			if (vectors != null)
			{
				for (int i = 0; i < vectors.Length; i++)
				{
					var vector = vectors[i];
					double x = vector.X;
					double y = vector.Y;

					MultiplyVector(ref x, ref y);

					vectors[i] = new Vector2(x, y);
				}
			}
		}

		public double Determinant
		{
			get
			{
				switch (_type)
				{
					case MatrixTypes.Identity:
					case MatrixTypes.Translation:
						return 1.0;
					case MatrixTypes.Scaling:
					case MatrixTypes.Scaling | MatrixTypes.Translation:
						return _m11 * _m22;
					default:
						return (_m11 * _m22) - (_m12 * _m21);
				}
			}
		}

		public bool HasInverse { get { return Determinant != 0; } }

		public void Invert()
		{
			double determinant = Determinant;

			if (determinant == 0)
			{
				throw new InvalidOperationException();
			}

			switch (_type)
			{
				case MatrixTypes.Identity:
					break;
				case MatrixTypes.Scaling:
					{
						_m11 = 1.0 / _m11;
						_m22 = 1.0 / _m22;
					}
					break;
				case MatrixTypes.Translation:
					_offsetX = -_offsetX;
					_offsetY = -_offsetY;
					break;
				case MatrixTypes.Scaling | MatrixTypes.Translation:
					{
						_m11 = 1.0 / _m11;
						_m22 = 1.0 / _m22;
						_offsetX = -_offsetX * _m11;
						_offsetY = -_offsetY * _m22;
					}
					break;
				default:
					{
						double invdet = 1.0 / determinant;
						SetMatrix(_m22 * invdet,
								  -_m12 * invdet,
								  -_m21 * invdet,
								  _m11 * invdet,
								  (_m21 * _offsetY - _offsetX * _m22) * invdet,
								  (_offsetX * _m12 - _m11 * _offsetY) * invdet,
								  MatrixTypes.Unknown);
					}
					break;
			}
		}

		public double M11
		{
			get
			{
				if (_type == MatrixTypes.Identity)
				{
					return 1.0;
				}
				else
				{
					return _m11;
				}
			}
			set
			{
				if (_type == MatrixTypes.Identity)
				{
					SetMatrix(value, 0,
							  0, 1,
							  0, 0,
							  MatrixTypes.Scaling);
				}
				else
				{
					_m11 = value;
					if (_type != MatrixTypes.Unknown)
					{
						_type |= MatrixTypes.Scaling;
					}
				}
			}
		}

		public double M12
		{
			get
			{
				if (_type == MatrixTypes.Identity)
				{
					return 0;
				}
				else
				{
					return _m12;
				}
			}
			set
			{
				if (_type == MatrixTypes.Identity)
				{
					SetMatrix(1, value,
							  0, 1,
							  0, 0,
							  MatrixTypes.Unknown);
				}
				else
				{
					_m12 = value;
					_type = MatrixTypes.Unknown;
				}
			}
		}

		public double M21
		{
			get
			{
				if (_type == MatrixTypes.Identity)
				{
					return 0;
				}
				else
				{
					return _m21;
				}
			}
			set
			{
				if (_type == MatrixTypes.Identity)
				{
					SetMatrix(1, 0,
							  value, 1,
							  0, 0,
							  MatrixTypes.Unknown);
				}
				else
				{
					_m21 = value;
					_type = MatrixTypes.Unknown;
				}
			}
		}

		public double M22
		{
			get
			{
				if (_type == MatrixTypes.Identity)
				{
					return 1.0;
				}
				else
				{
					return _m22;
				}
			}
			set
			{
				if (_type == MatrixTypes.Identity)
				{
					SetMatrix(1, 0,
							  0, value,
							  0, 0,
							  MatrixTypes.Scaling);
				}
				else
				{
					_m22 = value;
					if (_type != MatrixTypes.Unknown)
					{
						_type |= MatrixTypes.Scaling;
					}
				}
			}
		}

		public double OffsetX
		{
			get
			{
				if (_type == MatrixTypes.Identity)
				{
					return 0;
				}
				else
				{
					return _offsetX;
				}
			}
			set
			{
				if (_type == MatrixTypes.Identity)
				{
					SetMatrix(1, 0,
							  0, 1,
							  value, 0,
							  MatrixTypes.Translation);
				}
				else
				{
					_offsetX = value;
					if (_type != MatrixTypes.Unknown)
					{
						_type |= MatrixTypes.Translation;
					}
				}
			}
		}

		public double OffsetY
		{
			get
			{
				if (_type == MatrixTypes.Identity)
				{
					return 0;
				}
				else
				{
					return _offsetY;
				}
			}
			set
			{
				if (_type == MatrixTypes.Identity)
				{
					SetMatrix(1, 0,
							  0, 1,
							  0, value,
							  MatrixTypes.Translation);
				}
				else
				{
					_offsetY = value;
					if (_type != MatrixTypes.Unknown)
					{
						_type |= MatrixTypes.Translation;
					}
				}
			}
		}

		internal void MultiplyVector(ref double x, ref double y)
		{
			switch (_type)
			{
				case MatrixTypes.Identity:
				case MatrixTypes.Translation:
					return;
				case MatrixTypes.Scaling:
				case MatrixTypes.Scaling | MatrixTypes.Translation:
					x *= _m11;
					y *= _m22;
					break;
				default:
					double xadd = y * _m21;
					double yadd = x * _m12;
					x *= _m11;
					x += xadd;
					y *= _m22;
					y += yadd;
					break;
			}
		}

		internal void MultiplyPoint(ref double x, ref double y)
		{
			switch (_type)
			{
				case MatrixTypes.Identity:
					return;
				case MatrixTypes.Translation:
					x += _offsetX;
					y += _offsetY;
					return;
				case MatrixTypes.Scaling:
					x *= _m11;
					y *= _m22;
					return;
				case MatrixTypes.Scaling | MatrixTypes.Translation:
					x *= _m11;
					x += _offsetX;
					y *= _m22;
					y += _offsetY;
					break;
				default:
					double xadd = y * _m21 + _offsetX;
					double yadd = x * _m12 + _offsetY;
					x *= _m11;
					x += xadd;
					y *= _m22;
					y += yadd;
					break;
			}
		}

		internal static Matrix CreateRotationRadians(double angle)
		{
			return CreateRotationRadians(angle, 0, 0);
		}

		internal static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
		{
			Matrix matrix = new Matrix();

			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			double dx = (centerX * (1.0 - cos)) + (centerY * sin);
			double dy = (centerY * (1.0 - cos)) - (centerX * sin);

			matrix.SetMatrix(cos, sin,
							  -sin, cos,
							  dx, dy,
							  MatrixTypes.Unknown);

			return matrix;
		}

		internal static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
		{
			Matrix matrix = new Matrix();

			matrix.SetMatrix(scaleX, 0,
							 0, scaleY,
							 centerX - scaleX * centerX, centerY - scaleY * centerY,
							 MatrixTypes.Scaling | MatrixTypes.Translation);

			return matrix;
		}

		internal static Matrix CreateScaling(double scaleX, double scaleY)
		{
			Matrix matrix = new Matrix();
			matrix.SetMatrix(scaleX, 0,
							 0, scaleY,
							 0, 0,
							 MatrixTypes.Scaling);
			return matrix;
		}

		internal static Matrix CreateSkewRadians(double skewX, double skewY)
		{
			Matrix matrix = new Matrix();

			matrix.SetMatrix(1.0, Math.Tan(skewY),
							 Math.Tan(skewX), 1.0,
							 0.0, 0.0,
							 MatrixTypes.Unknown);

			return matrix;
		}

		/// <summary>
		/// Sets the transformation to the given translation specified by the offset vector.
		/// </summary>
		/// <param name='offsetX'>The offset in X</param>
		/// <param name='offsetY'>The offset in Y</param>
		internal static Matrix CreateTranslation(double offsetX, double offsetY)
		{
			Matrix matrix = new Matrix();

			matrix.SetMatrix(1, 0,
							 0, 1,
							 offsetX, offsetY,
							 MatrixTypes.Translation);

			return matrix;
		}

		static Matrix CreateIdentity()
		{
			Matrix matrix = new Matrix();
			matrix.SetMatrix(1, 0,
							 0, 1,
							 0, 0,
							 MatrixTypes.Identity);
			return matrix;
		}

		void SetMatrix(double m11, double m12,
							   double m21, double m22,
							   double offsetX, double offsetY,
							   MatrixTypes type)
		{
			_m11 = m11;
			_m12 = m12;
			_m21 = m21;
			_m22 = m22;
			_offsetX = offsetX;
			_offsetY = offsetY;
			_type = type;
		}

		void DeriveMatrixType()
		{
			_type = 0;

			if (!(_m21 == 0 && _m12 == 0))
			{
				_type = MatrixTypes.Unknown;
				return;
			}

			if (!(_m11 == 1 && _m22 == 1))
			{
				_type = MatrixTypes.Scaling;
			}

			if (!(_offsetX == 0 && _offsetY == 0))
			{
				_type |= MatrixTypes.Translation;
			}

			if (0 == (_type & (MatrixTypes.Translation | MatrixTypes.Scaling)))
			{
				_type = MatrixTypes.Identity;
			}
			return;
		}
	}

	internal static class MatrixUtil
	{
		internal static void TransformRect(ref Xamarin.Forms.Rectangle rect, ref Matrix matrix)
		{
			if (rect.IsEmpty)
			{
				return;
			}

			MatrixTypes matrixType = matrix._type;

			if (matrixType == MatrixTypes.Identity)
			{
				return;
			}

			// Scaling
			if (0 != (matrixType & MatrixTypes.Scaling))
			{
				rect.X *= matrix._m11;
				rect.Y *= matrix._m22;
				rect.Width *= matrix._m11;
				rect.Height *= matrix._m22;

				if (rect.Width < 0.0)
				{
					rect.X += rect.Width;
					rect.Width = -rect.Width;
				}

				if (rect.Height < 0.0)
				{
					rect.Y += rect.Height;
					rect.Height = -rect.Height;
				}
			}

			// Translation
			if (0 != (matrixType & MatrixTypes.Translation))
			{
				// X
				rect.X += matrix._offsetX;

				// Y
				rect.X += matrix._offsetY;
			}

			if (matrixType == MatrixTypes.Unknown)
			{
				Point point0 = matrix.Transform(new Point(rect.Right, rect.Top));
				Point point1 = matrix.Transform(new Point(rect.Right, rect.Top));
				Point point2 = matrix.Transform(new Point(rect.Right, rect.Bottom));
				Point point3 = matrix.Transform(new Point(rect.Left, rect.Bottom));

				rect.X = Math.Min(Math.Min(point0.X, point1.X), Math.Min(point2.X, point3.X));
				rect.Y = Math.Min(Math.Min(point0.Y, point1.Y), Math.Min(point2.Y, point3.Y));

				rect.Width = Math.Max(Math.Max(point0.X, point1.X), Math.Max(point2.X, point3.X)) - rect.X;
				rect.Height = Math.Max(Math.Max(point0.Y, point1.Y), Math.Max(point2.Y, point3.Y)) - rect.Y;
			}
		}

		internal static void MultiplyMatrix(ref Matrix matrix1, ref Matrix matrix2)
		{
			MatrixTypes type1 = matrix1._type;
			MatrixTypes type2 = matrix2._type;

			if (type2 == MatrixTypes.Identity)
			{
				return;
			}

			if (type1 == MatrixTypes.Identity)
			{
				matrix1 = matrix2;
				return;
			}

			if (type2 == MatrixTypes.Translation)
			{
				matrix1._offsetX += matrix2._offsetX;
				matrix1._offsetY += matrix2._offsetY;

				if (type1 != MatrixTypes.Unknown)
				{
					matrix1._type |= MatrixTypes.Translation;
				}

				return;
			}

			// Check for the first value being a translate
			if (type1 == MatrixTypes.Translation)
			{
				double offsetX = matrix1._offsetX;
				double offsetY = matrix1._offsetY;

				matrix1 = matrix2;

				matrix1._offsetX = offsetX * matrix2._m11 + offsetY * matrix2._m21 + matrix2._offsetX;
				matrix1._offsetY = offsetX * matrix2._m12 + offsetY * matrix2._m22 + matrix2._offsetY;

				if (type2 == MatrixTypes.Unknown)
				{
					matrix1._type = MatrixTypes.Unknown;
				}
				else
				{
					matrix1._type = MatrixTypes.Scaling | MatrixTypes.Translation;
				}
				return;
			}

			// trans1._type |  trans2._type
			//  7  6  5  4   |  3  2  1  0
			int combinedType = ((int)type1 << 4) | (int)type2;

			switch (combinedType)
			{
				case 34:  // S * S
						  // 2 multiplications
					matrix1._m11 *= matrix2._m11;
					matrix1._m22 *= matrix2._m22;
					return;

				case 35:  // S * S|T
					matrix1._m11 *= matrix2._m11;
					matrix1._m22 *= matrix2._m22;
					matrix1._offsetX = matrix2._offsetX;
					matrix1._offsetY = matrix2._offsetY;

					// Transform set to Translate and Scale
					matrix1._type = MatrixTypes.Translation | MatrixTypes.Scaling;
					return;

				case 50: // S|T * S
					matrix1._m11 *= matrix2._m11;
					matrix1._m22 *= matrix2._m22;
					matrix1._offsetX *= matrix2._m11;
					matrix1._offsetY *= matrix2._m22;
					return;

				case 51: // S|T * S|T
					matrix1._m11 *= matrix2._m11;
					matrix1._m22 *= matrix2._m22;
					matrix1._offsetX = matrix2._m11 * matrix1._offsetX + matrix2._offsetX;
					matrix1._offsetY = matrix2._m22 * matrix1._offsetY + matrix2._offsetY;
					return;
				case 36: // S * U
				case 52: // S|T * U
				case 66: // U * S
				case 67: // U * S|T
				case 68: // U * U
					matrix1 = new Matrix(
						matrix1._m11 * matrix2._m11 + matrix1._m12 * matrix2._m21,
						matrix1._m11 * matrix2._m12 + matrix1._m12 * matrix2._m22,

						matrix1._m21 * matrix2._m11 + matrix1._m22 * matrix2._m21,
						matrix1._m21 * matrix2._m12 + matrix1._m22 * matrix2._m22,

						matrix1._offsetX * matrix2._m11 + matrix1._offsetY * matrix2._m21 + matrix2._offsetX,
						matrix1._offsetX * matrix2._m12 + matrix1._offsetY * matrix2._m22 + matrix2._offsetY);
					return;
				default:
					break;
			}
		}

		internal static void PrependOffset(ref Matrix matrix, double offsetX, double offsetY)
		{
			if (matrix._type == MatrixTypes.Identity)
			{
				matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY)
				{
					_type = MatrixTypes.Translation
				};
			}
			else
			{
				matrix._offsetX += matrix._m11 * offsetX + matrix._m21 * offsetY;
				matrix._offsetY += matrix._m12 * offsetX + matrix._m22 * offsetY;

				if (matrix._type != MatrixTypes.Unknown)
				{
					matrix._type |= MatrixTypes.Translation;
				}
			}
		}
	}
}