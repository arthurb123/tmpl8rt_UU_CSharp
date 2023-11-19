using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace tmpl8rt_UU_CSharp {
    public class Cube : Primitive {
        public Vector4D<float>[] Corners { get; private set; }
        public Matrix4X4<float> Transform { get; private set; }

        private Matrix4X4<float> _inverseTransform = Matrix4X4<float>.Identity;

        public Cube(
            int id, 
            Vector3D<float> position, 
            Vector3D<float> size, 
            Matrix4X4<float> transform = default
        ) : base(id) {
            Corners = new Vector4D<float>[2] {
                new Vector4D<float>(position - 0.5f * size, 1f),
                new Vector4D<float>(position + 0.5f * size, 1f)
            };
            Transform = transform == default ? Matrix4X4<float>.Identity : transform;

            if (!Matrix4X4.Invert(Transform, out _inverseTransform))
                // TODO: Add better logging here
                Console.WriteLine("Failed to invert transform matrix");
        }
        
        public override bool Intersects(Ray ray)
        {
            Vector3D<float> origin = Vector3D.Transform(ray.Origin, _inverseTransform);
            Vector3D<float> direction = Vector3D.TransformNormal(ray.Direction, _inverseTransform);

            float rDx = 1f / direction.X, rDy = 1f / direction.Y, rDz = 1f / direction.Z;
            int signX = direction.X < 0f ? 1 : 0, signY = direction.Y < 0f ? 1 : 0, signZ = direction.Z < 0f ? 1 : 0;
            float tMin = (Corners[signX].X - origin.X) * rDx;
            float tMax = (Corners[1 - signX].X - origin.X) * rDx;
            float tyMin = (Corners[signY].Y - origin.Y) * rDy;
            float tyMax = (Corners[1 - signY].Y - origin.Y) * rDy;
            if (tMin > tyMax || tyMin > tMax) 
                return false;

            tMin = Scalar.Max(tMin, tyMin);
            tMax = Scalar.Min(tMax, tyMax);
            float tzMin = (Corners[signZ].Z - origin.Z) * rDz;
            float tzMax = (Corners[1 - signZ].Z - origin.Z) * rDz;
            if (tMin > tzMax || tzMin > tMax) 
                return false;

            tMin = Scalar.Max(tMin, tzMin);
            tMax = Scalar.Min(tMax, tzMax);
            if (tMin > 0) {
                if (tMin < ray.Distance) {
                    ray.RegisterHit(ID, tMin);
                    return true;
                }
            }
            else if (tMax > 0) {
                if (tMax < ray.Distance) {
                    ray.RegisterHit(ID, tMax);
                    return true;
                }
            }

            return false;
        }

        public override bool IsOccluded(Ray ray)
        {
            Vector4D<float> origin = Vector4D.Transform(ray.Origin, _inverseTransform);
            Vector4D<float> direction = Vector4D.Transform(ray.Direction, _inverseTransform);

            float rDx = 1f / direction.X, rDy = 1f / direction.Y, rDz = 1f / direction.Z;
            float t1 = (Corners[0].X - origin.X) * rDx, t2 = (Corners[1].X - origin.X) * rDx;
            float t3 = (Corners[0].Y - origin.Y) * rDy, t4 = (Corners[1].Y - origin.Y) * rDy;
            float t5 = (Corners[0].Z - origin.Z) * rDz, t6 = (Corners[1].Z - origin.Z) * rDz;
            float tMin = Scalar.Max(Scalar.Max(Scalar.Min(t1, t2), Scalar.Min(t3, t4)), Scalar.Min(t5, t6));
            float tMax = Scalar.Min(Scalar.Min(Scalar.Max(t1, t2), Scalar.Max(t3, t4)), Scalar.Max(t5, t6));

            return tMax > 0 && tMin < tMax && tMin < ray.Distance;
        }

        public override Vector3D<float> GetNormal(Vector3D<float> point)
        {
            // Transform intersection point to object space
            Vector4D<float> p = Vector4D.Transform(new Vector4D<float>(point, 1f), _inverseTransform);

            // Determine the normal in object space
            Vector3D<float> normal = new Vector3D<float>(-1f, 0f, 0f);
            float d0 = Scalar.Abs(p.X - Corners[0].X), d1 = Scalar.Abs(p.X - Corners[1].X);
            float d2 = Scalar.Abs(p.Y - Corners[0].Y), d3 = Scalar.Abs(p.Y - Corners[1].Y);
            float d4 = Scalar.Abs(p.Z - Corners[0].Z), d5 = Scalar.Abs(p.Z - Corners[1].Z);
            float minDist = d0;
            if (d1 < minDist) { 
                minDist = d1;
                normal.X = 1f;
            }
            if (d2 < minDist) { 
                minDist = d2;
                normal = new Vector3D<float>(0f, -1f, 0f);
            }
            if (d3 < minDist) { 
                minDist = d3;
                normal = new Vector3D<float>(0f, 1f, 0f);
            }
            if (d4 < minDist) { 
                minDist = d4;
                normal = new Vector3D<float>(0f, 0f, -1f);
            }
            if (d5 < minDist) { 
                minDist = d5;
                normal = new Vector3D<float>(0f, 0f, 1f);
            }

            // Return normal in world space
            return Vector3D.Transform(normal, Transform);
        }

        public override Vector4D<float> GetAlbedo(Vector3D<float> point)
        {
            // Return red
            return new Vector4D<float>(0.9f, 0.1f, 0.1f, 1f);
        }

        public void SetTransform(Matrix4X4<float> transform)
        {
            Transform = transform;
            if (!Matrix4X4.Invert(Transform, out _inverseTransform)) {
                // TODO: Add better logging here
                Console.WriteLine("Failed to invert transform matrix");
            }
        }

        public override void Dispose()
        {
            
        }
    }
}