using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public class Plane : Primitive {
        public Vector3D<float> Normal { get; private set; }
        public float Distance { get; private set; }
        public Surface? Surface { get; private set; }

        public Plane(int id, Vector3D<float> normal, float distance, Surface? surface = null) : base(id) {
            Normal = normal;
            Distance = distance;
            Surface = surface;
        }

        public override bool Intersects(ref Ray ray)
        {
            float t = -(Vector3D.Dot(ray.Origin, Normal) + Distance) / Vector3D.Dot(ray.Direction, Normal);
            if (t < ray.Distance && t > 0.0f) {
                ray.RegisterHit(ID, t);
                return true;
            }

            return false;
        }

        public override bool IsOccluded(Ray ray)
        {
            return false;
        }

        public override Vector3D<float> GetNormal(Vector3D<float> point)
        {
            return Normal;
        }

        public override Vector4D<float> GetAlbedo(Vector3D<float> point)
        {
            // If there exists no surface, return checkerboard
            if (Surface == null) {
                int ix = (int)(point.X * 2 + 96.01f);
                int iz = (int)(point.Z * 2 + 96.01f);
                // Add deliberate aliasing to two tile
                if (ix == 98 && iz == 98) {
                    ix = (int)(point.X * 32.01f); 
                    iz = (int)(point.Z * 32.01f);
                }
                if (ix == 94 && iz == 98) {
                    ix = (int)(point.X * 64.01f); 
                    iz = (int)(point.Z * 64.01f);
                }
                float c = ((ix + iz) & 1) == 1 ? 1f : 0.3f;
                return new Vector4D<float>(c, c, c, 1f);
            }

            // Because a plane is infinite, we need to map a surface to it
            // based on a scale factor
            float mappingScale = 480f;

            // Find the smallest absolute component of the normal and set the arbitrary vector accordingly
            Vector3D<float> arbitraryVector;
            if (Math.Abs(Normal.X) < Math.Abs(Normal.Y) && Math.Abs(Normal.X) < Math.Abs(Normal.Z))
                arbitraryVector = Vector3D<float>.UnitX;
            else if (Math.Abs(Normal.Y) < Math.Abs(Normal.Z))
                arbitraryVector = Vector3D<float>.UnitY;
            else
                arbitraryVector = Vector3D<float>.UnitZ;

            // Cross product to find the orthogonal tangent
            Vector3D<float> tangent = Vector3D.Normalize(Vector3D.Cross(arbitraryVector, Normal));

            // Compute bitangent
            Vector3D<float> bitangent = Vector3D.Normalize(Vector3D.Cross(Normal, tangent));

            // Check the orientation of the plane based on its normal vector
            bool isRightPlane = Normal.X > 0.99f; // Assuming right-facing normals are close to (1, 0, 0)
            bool isLeftPlane = Normal.X < -0.99f; // Assuming left-facing normals are close to (-1, 0, 0)

            if (isLeftPlane) {
                // Rotate tangent and bitangent by 90 degrees
                Vector3D<float> originalTangent = tangent;
                tangent = bitangent;
                bitangent = -originalTangent;
            }
            if (isRightPlane) {
                Vector3D<float> originalTangent = tangent;
                tangent = -bitangent;
                bitangent = originalTangent;
            }

            // Calculate u and v coordinates based on the plane's orientation
            float u = 1f - (Vector3D.Dot(point, tangent) / Surface.Width * mappingScale);
            float v = 1f - (Vector3D.Dot(point, bitangent) / Surface.Height * mappingScale);

            // Get albedo from surface
            return Surface.GetAlbedo(u, v);
        }

        public override void Dispose()
        {
            // Dispose possible surface
            Surface?.Dispose();
        }
    }
}