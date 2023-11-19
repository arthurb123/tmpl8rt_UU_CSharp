using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public class Quad : Primitive
    {
        public float Size { get; private set; }
        public Matrix4X4<float> Transform { get; private set; }
        public Matrix4X4<float> InverseTransform { get; private set; }

        public Quad(int id, float size, Matrix4X4<float> transform = default)
            : base(id)
        {
            Size = size * 0.5f;
            Transform = transform == default ? Matrix4X4<float>.Identity : transform;
            InverseTransform = Matrix4X4.Invert(Transform, out var inv) ? inv : Matrix4X4<float>.Identity;
        }

        public override bool Intersects(Ray ray)
        {
            float Oy = InverseTransform.M21 * ray.Origin.X + InverseTransform.M22 * ray.Origin.Y + InverseTransform.M23 * ray.Origin.Z + InverseTransform.M24;
            float Dy = InverseTransform.M21 * ray.Direction.X + InverseTransform.M22 * ray.Direction.Y + InverseTransform.M23 * ray.Direction.Z;
            float t = Oy / -Dy;

            if (t < ray.Distance && t > 0.0f)
            {
                float Ox = InverseTransform.M11 * ray.Origin.X + InverseTransform.M12 * ray.Origin.Y + InverseTransform.M13 * ray.Origin.Z + InverseTransform.M14;
                float Oz = InverseTransform.M31 * ray.Origin.X + InverseTransform.M32 * ray.Origin.Y + InverseTransform.M33 * ray.Origin.Z + InverseTransform.M34;
                float Dx = InverseTransform.M11 * ray.Direction.X + InverseTransform.M12 * ray.Direction.Y + InverseTransform.M13 * ray.Direction.Z;
                float Dz = InverseTransform.M31 * ray.Direction.X + InverseTransform.M32 * ray.Direction.Y + InverseTransform.M33 * ray.Direction.Z;
                float Ix = Ox + t * Dx, Iz = Oz + t * Dz;

                if (Ix > -Size && Ix < Size && Iz > -Size && Iz < Size)
                {
                    ray.RegisterHit(ID, t);
                    return true;
                }
            }

            return false;
        }

        public override bool IsOccluded(Ray ray)
        {
            float Oy = InverseTransform.M21 * ray.Origin.X + InverseTransform.M22 * ray.Origin.Y + InverseTransform.M23 * ray.Origin.Z + InverseTransform.M24;
            float Dy = InverseTransform.M21 * ray.Direction.X + InverseTransform.M22 * ray.Direction.Y + InverseTransform.M23 * ray.Direction.Z;
            float t = Oy / -Dy;

            if (t < ray.Distance && t > 0.0f)
            {
                float Ox = InverseTransform.M11 * ray.Origin.X + InverseTransform.M12 * ray.Origin.Y + InverseTransform.M13 * ray.Origin.Z + InverseTransform.M14;
                float Oz = InverseTransform.M31 * ray.Origin.X + InverseTransform.M32 * ray.Origin.Y + InverseTransform.M33 * ray.Origin.Z + InverseTransform.M34;
                float Dx = InverseTransform.M11 * ray.Direction.X + InverseTransform.M12 * ray.Direction.Y + InverseTransform.M13 * ray.Direction.Z;
                float Dz = InverseTransform.M31 * ray.Direction.X + InverseTransform.M32 * ray.Direction.Y + InverseTransform.M33 * ray.Direction.Z;
                float Ix = Ox + t * Dx, Iz = Oz + t * Dz;

                return Ix > -Size && Ix < Size && Iz > -Size && Iz < Size;
            }

            return false;
        }

        public override Vector3D<float> GetNormal(Vector3D<float> point)
        {
            return new Vector3D<float>(-Transform.M12, -Transform.M22, -Transform.M32);
        }

        public override Vector4D<float> GetAlbedo(Vector3D<float> point)
        {
            return new Vector4D<float>(1f);
        }

        public void SetTransform(Matrix4X4<float> transform)
        {
            Transform = transform;
            InverseTransform = Matrix4X4.Invert(Transform, out var inv) ? inv : Matrix4X4<float>.Identity;
        }

        public override void Dispose()
        {
            
        }
    }
}