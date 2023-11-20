using System.Numerics;
using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public class Torus : Primitive
    {
        public float Rt2 { get; private set; }
        public float Rc2 { get; private set; }
        public float R2 { get; private set; }
        public Matrix4X4<float> Transform { get; private set; }

        private Matrix4X4<float> _inverseTransform;

        public Torus(int id, float a, float b, Matrix4X4<float> transform = default) : base(id)
        {
            Rc2 = a * a;
            Rt2 = b * b;
            R2 = Scalar.Sqrt<float>(a + b);
            Transform = transform == default ? Matrix4X4<float>.Identity : transform;
            if (!Matrix4X4.Invert(Transform, out _inverseTransform)) {
                // TODO: Add better loggin here
                Console.WriteLine("Failed to invert transform matrix");
            }
        }

        public override bool Intersects(ref Ray ray)
        {
            // Via: https://www.shadertoy.com/view/4sBGDy
            Vector3D<float> O = Vector3D.Transform(ray.Origin, _inverseTransform);
            Vector3D<float> D = Vector3D.TransformNormal(ray.Direction, _inverseTransform);

            // Extension rays need double precision for the quadratic solver
            double po = 1, m = Vector3D.Dot(O, O), k3 = Vector3D.Dot(O, D), k32 = k3 * k3;

            // Bounding sphere test
            double v = k32 - m + R2;
            if (v < 0) 
                return false;

            // Setup torus intersection
            double k = (m - Rt2 - Rc2) * 0.5, k2 = k32 + Rc2 * D.Z * D.Z + k;
            double k1 = k * k3 + Rc2 * O.Z * D.Z, k0 = k * k + Rc2 * O.Z * O.Z - Rc2 * Rt2;

            // Solve quadratic equation
            if (Math.Abs(k3 * (k32 - k2) + k1) < 0.0001)
            {
                double oldK3 = k3;
                k3 = k1;
                k1 = oldK3;

                po = -1;
                k0 = 1 / k0;
                k1 *= k0;
                k2 *= k0;
                k3 *= k0;
                k32 = k3 * k3;
            }

            double c2 = 2 * k2 - 3 * k32, c1 = k3 * (k32 - k2) + k1;
            double c0 = k3 * (k3 * (-3 * k32 + 4 * k2) - 8 * k1) + 4 * k0;
            c2 *= 0.33333333333;
            c1 *= 2;
            c0 *= 0.33333333333;

            double Q = c2 * c2 + c0, R = 3 * c0 * c2 - c2 * c2 * c2 - c1 * c1;
            double h = R * R - Q * Q * Q, z;

            if (h < 0)
            {
                double sQ = Scalar.Sqrt(Q);
                z = 2 * sQ * Scalar.Cos(Scalar.Acos(R / (sQ * Q)) * 0.333333333);
            }
            else
            {
                double sQ = CbrtFast(Scalar.Sqrt(h) + Scalar.Abs(R));
                z = Scalar.Sign(R) * Scalar.Abs(sQ + Q / sQ);
            }

            z = c2 - z;
            double d1 = z - 3 * c2, d2 = z * z - 3 * c0;

            if (Scalar.Abs(d1) < 1e-8)
            {
                if (d2 < 0) 
                    return false;

                d2 = Scalar.Sqrt(d2);
            }
            else
            {
                if (d1 < 0) 
                    return false;

                d1 = Scalar.Sqrt(d1 * 0.5);
                d2 = c1 / d1;
            }

            double t = 1e20;
            h = d1 * d1 - z + d2;

            if (h > 0)
            {
                h = Scalar.Sqrt(h);
                double t1 = -d1 - h - k3, t2 = -d1 + h - k3;
                t1 = po < 0 ? 2 / t1 : t1;
                t2 = po < 0 ? 2 / t2 : t2;
                if (t1 > 0) t = t1;
                if (t2 > 0) t = Scalar.Min(t, t2);
            }

            h = d1 * d1 - z - d2;
            if (h > 0)
            {
                h = Scalar.Sqrt(h);
                double t1 = d1 - h - k3, t2 = d1 + h - k3;
                t1 = po < 0 ? 2 / t1 : t1;
                t2 = po < 0 ? 2 / t2 : t2;
                if (t1 > 0) t = Scalar.Min(t, t1);
                if (t2 > 0) t = Scalar.Min(t, t2);
            }

            float ft = (float)t;
            if (ft > 0 && ft < ray.Distance)
            {
                ray.RegisterHit(ID, ft);
                return true;
            }

            return false;
        }

        public override bool IsOccluded(Ray ray)
        {
            // Via: https://www.shadertoy.com/view/4sBGDy
            Vector3D<float> O = Vector3D.Transform(ray.Origin, _inverseTransform);
            Vector3D<float> D = Vector3D.TransformNormal(ray.Direction, _inverseTransform);

            // Extension rays need double precision for the quadratic solver
            double po = 1, m = Vector3D.Dot(O, O), k3 = Vector3D.Dot(O, D), k32 = k3 * k3;

            // Bounding sphere test
            double v = k32 - m + R2;
            if (v < 0) 
                return false;

            // Setup torus intersection
            double k = (m - Rt2 - Rc2) * 0.5, k2 = k32 + Rc2 * D.Z * D.Z + k;
            double k1 = k * k3 + Rc2 * O.Z * D.Z, k0 = k * k + Rc2 * O.Z * O.Z - Rc2 * Rt2;

            // Solve quadratic equation
            if (Math.Abs(k3 * (k32 - k2) + k1) < 0.0001)
            {
                double oldK3 = k3;
                k3 = k1;
                k1 = oldK3;

                po = -1;
                k0 = 1 / k0;
                k1 *= k0;
                k2 *= k0;
                k3 *= k0;
                k32 = k3 * k3;
            }

            double c2 = 2 * k2 - 3 * k32, c1 = k3 * (k32 - k2) + k1;
            double c0 = k3 * (k3 * (-3 * k32 + 4 * k2) - 8 * k1) + 4 * k0;
            c2 *= 0.33333333333;
            c1 *= 2;
            c0 *= 0.33333333333;

            double Q = c2 * c2 + c0, R = 3 * c0 * c2 - c2 * c2 * c2 - c1 * c1;
            double h = R * R - Q * Q * Q, z;

            if (h < 0)
            {
                double sQ = Scalar.Sqrt(Q);
                z = 2 * sQ * Scalar.Cos(Scalar.Acos(R / (sQ * Q)) * 0.333333333);
            }
            else
            {
                double sQ = CbrtFast(Scalar.Sqrt(h) + Scalar.Abs(R));
                z = Scalar.Sign(R) * Scalar.Abs(sQ + Q / sQ);
            }

            z = c2 - z;
            double d1 = z - 3 * c2, d2 = z * z - 3 * c0;

            if (Scalar.Abs(d1) < 1e-8)
            {
                if (d2 < 0) 
                    return false;

                d2 = Scalar.Sqrt(d2);
            }
            else
            {
                if (d1 < 0) 
                    return false;

                d1 = Scalar.Sqrt(d1 * 0.5);
                d2 = c1 / d1;
            }

            h = d1 * d1 - z + d2;
            if (h > 0)
            {
                double t1 = -d1 - Scalar.Sqrt<double>(h) - k3;
                t1 = po < 0 ? 2 / t1 : t1;
                if (t1 > 0 && t1 < ray.Distance)
                    return true;
            }

            h = d1 * d1 - z - d2;
            if (h > 0)
            {
                double t1 = d1 - Scalar.Sqrt<double>(h) - k3;
                t1 = po < 0 ? 2 / t1 : t1;
                if (t1 > 0 && t1 < ray.Distance)
                    return true;
            }

            return false;
        }

        public override Vector3D<float> GetNormal(Vector3D<float> point)
        {
            Vector3D<float> L = Vector3D.Transform(point, _inverseTransform);
            Vector3D<float> N = L * (Vector3D.Subtract<float>(new Vector3D<float>(Vector3D.Dot(L, L) - Rt2), Vector3D.Multiply<float>(new Vector3D<float>(1, 1, -1), Rc2)));
            return Vector3D.TransformNormal(N, Transform);
        }

        public override Vector4D<float> GetAlbedo(Vector3D<float> point)
        {
            // Return blue
            return new Vector4D<float>(0.1f, 0.1f, 0.9f, 1f);
        }

        public override void Dispose()
        {
            
        }

        private float CbrtFast(float n)
        {
            float x1 = n / 10.0f, x2 = 1.0f;
            int turn = 0;
            while (MathF.Abs(x1 - x2) > 0.00000001 && turn++ < 100) {
                x1 = x2;
                x2 = (2.0f / 3.0f * x1) + (n / (3.0f * x1 * x1));
            }
            return x2;
        }

        private double CbrtFast(double n)
        {
            double x1 = n / 10.0f, x2 = 1.0f;
            int turn = 0;
            while (Math.Abs(x1 - x2) > 0.00000001 && turn++ < 100) {
                x1 = x2;
                x2 = (2.0f / 3.0f * x1) + (n / (3.0f * x1 * x1));
            }
            return x2;
        }
    }

}