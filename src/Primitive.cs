using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public abstract class Primitive : IDisposable {
        public int ID { get; private set; }

        public Primitive(int id) {
            ID = id;
        }

        public abstract bool Intersects(Ray ray);
        public abstract bool IsOccluded(Ray ray);
        public abstract Vector3D<float> GetNormal(Vector3D<float> point);
        public abstract Vector4D<float> GetAlbedo(Vector3D<float> point);
        public abstract void Dispose();
    }
}