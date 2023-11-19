using System.Diagnostics;
using Silk.NET.Maths;

namespace tmpl8rt_UU_CSharp {
    public class Scene : IDisposable {
        public const float PI = 3.14159265358979323846264f;

        public Quad[] Lights { get; private set; }
        public Primitive[] Objects { get; private set; }
        public float AnimationTime { get; private set; }

        // Empty constructor generates the sample scene
        // from the assignment template
        public Scene() {
            // 0: Four lights
            Lights = new Quad[4] {
                new Quad(id: 0, size: 0.5f),
                new Quad(id: 0, size: 0.5f),
                new Quad(id: 0, size: 0.5f),
                new Quad(id: 0, size: 0.5f)
            };

            // The sample scene holds a total of ten objects
            int objectId = 0;
            Objects = new Primitive[10];

            // Create surfaces
            Surface tilesSurface = new Surface("assets/tiles.jpg");
            Surface wood1Surface = new Surface("assets/wood1.jpg");
            Surface wood2Surface = new Surface("assets/wood2.jpg");

            // NOTE: For now we sort the objects on what order we want to render them
            // 1: Plane floor
            Objects[objectId++] = new Plane(id: 0, normal: new Vector3D<float>(0f, 1, 0f), distance: 1f);
            // 2: Plane left wall
            Objects[objectId++] = new Plane(id: 1, normal: new Vector3D<float>(1, 0f, 0f), distance: 3f, surface: wood1Surface);
            // 3: Plane right wall
            Objects[objectId++] = new Plane(id: 2, normal: new Vector3D<float>(-1, 0f, 0f), distance: 2.99f, surface: wood2Surface);
            // 4: Plane ceiling
            Objects[objectId++] = new Plane(id: 3, normal: new Vector3D<float>(0f, -1, 0f), distance: 2f);
            // 5: Plane front wall
            Objects[objectId++] = new Plane(id: 4, normal: new Vector3D<float>(0f, 0f, 1), distance: 3f, surface: tilesSurface);
            // 6: Plane back wall
            Objects[objectId++] = new Plane(id: 5, normal: new Vector3D<float>(0f, 0f, -1), distance: 3.99f, surface: tilesSurface);
            // 7: Rounded corners
            Objects[objectId++] = new Sphere(id: 6, center: new Vector3D<float>(0f, 2.5f, -3.07f), radius: 8f);

            // 8: Bouncing ball
            Objects[objectId++] = new Sphere(id: 7, center: Vector3D<float>.Zero, radius: 0.6f);
            // 9: Cube
            Objects[objectId++] = new Cube(id: 8, position: Vector3D<float>.Zero, size: new Vector3D<float>(1.15f, 1.15f, 1.15f));
            // 10: Torus
            Objects[objectId++] = new Torus(
                id: 9, 
                a: 0.8f,
                b: 0.25f,
                transform: Matrix4X4.CreateRotationX(PI / 4) * Matrix4X4.CreateTranslation(new Vector3D<float>(-0.25f, 0, 2f))
            );

            // Set time to zero
            SetTime(0);
        }

        public void SetTime(float t) {
            AnimationTime = t;

            // The four lights are stationary
            Lights[0].SetTransform(Matrix4X4.CreateTranslation(new Vector3D<float>(-1f, 1.5f, -1f)));
            Lights[1].SetTransform(Matrix4X4.CreateTranslation(new Vector3D<float>(1f, 1.5f, -1f)));
            Lights[2].SetTransform(Matrix4X4.CreateTranslation(new Vector3D<float>(1f, 1.5f, 1f)));
            Lights[3].SetTransform(Matrix4X4.CreateTranslation(new Vector3D<float>(-1f, 1.5f, 1f)));

            // Make the cube spin (remember, the cube is object id 3, therefore at index 2)
            Matrix4X4<float> M2base = Matrix4X4.CreateRotationZ(PI / 4) * Matrix4X4.CreateRotationX(PI / 4);
            Matrix4X4<float> M2 =  M2base * Matrix4X4.CreateRotationY(t * 0.5f) * Matrix4X4.CreateTranslation<float>(new Vector3D<float>(1.8f, 0f, 2.5f));
            ((Cube)Objects[8]).SetTransform(M2);

            // Make the sphere bounce, such that the tm value
            // is clamped between 0 and 1 and then back to 0
            float tm = t % 2f;
            if (tm > 1f) {
                tm = 2f - tm;
            }
            Vector3D<float> spherePosition = new Vector3D<float>(-1.8f, -0.4f + tm, 1f);
            ((Sphere)Objects[7]).SetPosition(spherePosition);
        }

        public Vector3D<float> RandomPointOnLight(float r0, float r1) {
            // Select a random light and use that
            int lightID = (int)(r0 * 4f);
            Quad light = Lights[lightID];

            // Renormalize r0 for reuse
            float stratum = lightID * 0.25f;
            float r2 = (r0 - stratum) / (1 - stratum);

            // Get a random position on the selected quad
            float size = light.Size;
            Vector3D<float> corner1 = Vector3D.Transform(new Vector3D<float>(-size, 0, -size), light.Transform);
            Vector3D<float> corner2 = Vector3D.Transform(new Vector3D<float>(size, 0, -size), light.Transform);
            Vector3D<float> corner3 = Vector3D.Transform(new Vector3D<float>(-size, 0, size), light.Transform);
            return corner1 + r2 * (corner2 - corner1) + r1 * (corner3 - corner1);
        }

        public void EvaluateRay(Ray ray) {
            // Check for intersections
            foreach (Primitive obj in Objects)
                obj.Intersects(ray);
        }

        public void Dispose() {
            foreach (Quad light in Lights) 
                light.Dispose();
            foreach (Primitive obj in Objects) 
                obj.Dispose();
        }
    }
}