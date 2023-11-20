using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public class Ray {
        protected const float MAXIMUM_DISTANCE = 1e34f;

        public Vector3D<float> Origin { get; private set; }
        public Vector3D<float> Direction { get; private set; }
        public float Distance { get; private set; } = MAXIMUM_DISTANCE;

        public int LastHitID { get; private set; } = -1;
        public HitType LastHitType { get; private set; }
        public bool InsideMedium { get; private set; } = false;

        public Ray(Vector3D<float> origin, Vector3D<float> direction, float distance = MAXIMUM_DISTANCE) {
            Origin = origin;
            Direction = Vector3D.Normalize(direction);
            Distance = distance;
        }

        public void RegisterHit(int id, float distance) {
            Distance = distance;
            LastHitID = id;
        }

        public void RegisterHitType(HitType hitType) {
            LastHitType = hitType;
        }

        public Vector3D<float> GetIntersectionPoint() {
            return Origin + Distance * Direction;
        }
    }

    public enum HitType {
        Object = 0,
        Light
    };
}