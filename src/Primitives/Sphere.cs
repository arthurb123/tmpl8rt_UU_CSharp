using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public class Sphere : Primitive {
        public Vector3D<float> Center { get; private set; }
        public float Radius { get; private set; }

        private float _radiusSquared;
        private float _inversedRadius;

        public Sphere(int id, Vector3D<float> center, float radius) : base(id) {
            Center = center;
            Radius = radius;

            _radiusSquared = radius * radius;
            _inversedRadius = 1.0f / radius;
        }

        public override bool Intersects(Ray ray)
        {
            Vector3D<float> oc = ray.Origin - Center;
            float b = Vector3D.Dot(oc, ray.Direction);
            float c = Vector3D.Dot(oc, oc) - _radiusSquared;
            float t, d = b * b - c;
            if (d <= 0.0f) 
                return false;

            d = Scalar.Sqrt(d);
            t = -b - d;
            if (t < ray.Distance && t > 0.0f) {
                ray.RegisterHit(ID, t);
                return true;
            }
            if (c > 0.0f) 
                return false;

            t = d - b;
            if (t < ray.Distance && t > 0.0f) {
                ray.RegisterHit(ID, t);
                return true;
            }

            return false;
        }

        public override bool IsOccluded(Ray ray)
        {
            Vector3D<float> oc = ray.Origin - Center;
            float b = Vector3D.Dot(oc, ray.Direction);
            float c = Vector3D.Dot(oc, oc) - _radiusSquared;
            float t, d = b * b - c;
            if (d <= 0.0f) 
                return false;

            d = Scalar.Sqrt(d);
            t = -b - d;
            bool hit = t < ray.Distance && t > 0.0f;
            return hit;
        }

        public override Vector3D<float> GetNormal(Vector3D<float> point)
        {
            return (point - Center) * _inversedRadius;
        }

        public override Vector4D<float> GetAlbedo(Vector3D<float> point)
        {
            // Return green
            return new Vector4D<float>(0.1f, 0.9f, 0.1f, 1f);
        }

        public override void Dispose()
        {
            
        }

        public void SetPosition(Vector3D<float> position)
        {
            Center = position;
        }
    }
}