using SharpDX;

namespace Microsoft.Maui.Graphics.SharpDX
{
    // ReSharper disable once InconsistentNaming
    public static class Matrix3x2Extensions
    {
        public static Matrix3x2 Scale(this Matrix3x2 target, float sx, float sy)
        {
            return Matrix3x2.Multiply(Matrix3x2.Scaling(sx, sy), target);
            /* target.M11 *= sx;
            target.M22 *= sy;
            return target;*/
        }

        public static Matrix3x2 Translate(this Matrix3x2 target, float dx, float dy)
        {
            return Matrix3x2.Multiply(Matrix3x2.Translation(dx, dy), target);
            /*target.M31 += dx;
            target.M32 += dy;
            return target;*/
        }

        public static Matrix3x2 Rotate(this Matrix3x2 target, float radians)
        {
            Matrix3x2 vMatrix = Matrix3x2.Multiply(Matrix3x2.Rotation(radians), target);
            /* target.M31 += dx;
            target.M32 += dy;*/
            return vMatrix;
        }
    }
}
