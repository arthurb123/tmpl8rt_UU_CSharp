using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.Input;

namespace tmpl8rt_UU_CSharp {
    public class Camera {
        public const float SPEED = 1.25f;
        public const int WIDTH = 1024;
        public const int HEIGHT = 640;
        public const float ASPECT_RATIO = (float)WIDTH / HEIGHT;

        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Target { get; private set; }

        private Vector3D<float> _topLeft;
        private Vector3D<float> _topRight;
        private Vector3D<float> _bottomLeft;

        public Camera() {
            Position = new Vector3D<float>(0, 0, -2);
            Target = new Vector3D<float>(0, 0, -1);
            
            _topLeft = new Vector3D<float>(-ASPECT_RATIO, 1, 0);
            _topRight = new Vector3D<float>(ASPECT_RATIO, 1, 0);
            _bottomLeft = new Vector3D<float>(-ASPECT_RATIO, -1, 0);
        }

        public unsafe void* AllocateBuffer() {
            uint size = WIDTH * HEIGHT * 4 * sizeof(float);
            return Marshal.AllocHGlobal((int)size).ToPointer();
        }

        public Ray GetPrimaryRay(float x, float y) {
            float u = x * (1f / WIDTH);
            float v = y * (1f / HEIGHT);

            Vector3D<float> P = _topLeft + u * (_topRight - _topLeft) + v * (_bottomLeft - _topLeft);
            return new Ray(Position, Vector3D.Normalize(P - Position));
        }

        public void HandleInput(IKeyboard keyboard, float deltaTime) {
            float speed = SPEED * deltaTime;
            Vector3D<float> ahead = Vector3D.Normalize(Target - Position);
            Vector3D<float> right = Vector3D.Normalize(Vector3D.Cross(Vector3D<float>.UnitY, ahead));
            Vector3D<float> up = Vector3D.Normalize(Vector3D.Cross(ahead, right));

            if (keyboard.IsKeyPressed(Key.ShiftLeft))
                speed *= 2f;

            bool changed = false;
            if (keyboard.IsKeyPressed(Key.W)) {
                Position += speed * ahead;
                changed = true;
            }
            if (keyboard.IsKeyPressed(Key.S)) {
                Position -= speed * ahead;
                changed = true;
            }
            if (keyboard.IsKeyPressed(Key.A)) {
                Position -= speed * right;
                changed = true;
            }
            if (keyboard.IsKeyPressed(Key.D)) {
                Position += speed * right;
                changed = true;
            }
            Target = Position + ahead;

            if (keyboard.IsKeyPressed(Key.Up)) {
                Target -= speed * up;
                changed = true;
            }
            if (keyboard.IsKeyPressed(Key.Down)) {
                Target += speed * up;
                changed = true;
            }
            if (keyboard.IsKeyPressed(Key.Left)) {
                Target -= speed * right;
                changed = true;
            }
            if (keyboard.IsKeyPressed(Key.Right)) {
                Target += speed * right;
                changed = true;
            }

            if (!changed)
                return;

            ahead = Vector3D.Normalize(Target - Position);
            up = Vector3D.Normalize(Vector3D.Cross(ahead, right));
            right = Vector3D.Normalize(Vector3D.Cross(up, ahead));
            _topLeft = Position + 2 * ahead - ASPECT_RATIO * right + up;
            _topRight = Position + 2 * ahead + ASPECT_RATIO * right + up;
            _bottomLeft = Position + 2 * ahead - ASPECT_RATIO * right - up;
        }
    }
}