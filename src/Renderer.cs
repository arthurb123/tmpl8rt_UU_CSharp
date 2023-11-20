using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Silk.NET.Vulkan;

namespace tmpl8rt_UU_CSharp {
    public static class Renderer {
        private static int _degreeOfParallelism = Environment.ProcessorCount;

        public static unsafe void RenderScene(Scene scene, Camera camera, void* bufferPtr) {
            // NOTE: This might be useful for later tasks
            // Create observable buffer
            // return Observable.Create<Vector3D<float>[]>((observer, token) => {
            //     return Task.Run(() => {
            //         // Create a ray for each pixel
            //         Parallel.For(0, Camera.HEIGHT, (int y) => {
            //             Parallel.For(0, Camera.WIDTH, (int x) => {
            //                 // Check if we should cancel
            //                 if (token.IsCancellationRequested)
            //                     return;

            //                 // Get the primary ray
            //                 Ray ray = camera.GetPrimaryRay(x, y);

            //                 // Trace the ray
            //                 Vector3D<float> color = Trace(scene, ray);

            //                 // Set the color in the buffer
            //                 lock (buffer) {
            //                     buffer[y * Camera.WIDTH + x] = color;
            //                 }

            //                 // Report progress
            //                 observer.OnNext(buffer);
            //             });
            //         });

            //         // Report completion
            //         observer.OnCompleted();
            //     });
            // });

            // Setup parallel options
            ParallelOptions options = new ParallelOptions() {
                MaxDegreeOfParallelism = _degreeOfParallelism
            };
            
            //Create a ray for each pixel
            float* buffer = (float*)bufferPtr;
            Parallel.For(0, Camera.HEIGHT, options, (int y) => {
                // Calculate the pointer offset for the current row
                float* row = buffer + y * Camera.WIDTH * 4;
                for (int x = 0; x < Camera.WIDTH; x++) {
                    // Get the primary ray
                    Ray ray = camera.GetPrimaryRay(x, y);

                    // Trace the ray
                    Vector4D<float> color = Trace(scene, ray);

                    // Set the color in the buffer
                    float* pixel = row + x * 4;
                    pixel[0] = color.X;
                    pixel[1] = color.Y;
                    pixel[2] = color.Z;
                    pixel[3] = color.W;
                }
            });
        }

        public static Vector4D<float> Trace(Scene scene, Ray ray) {
            // Evaluate ray in scene
            scene.EvaluateRay(ray);

            // Check if ray hit something
            if (ray.LastHitID == -1)
                return Vector4D<float>.Zero;

            // Get the intersection point
            Vector3D<float> intersectionPoint = ray.GetIntersectionPoint();

            // Return albedo for now
            return scene.GetAlbedo(intersectionPoint, ray.LastHitID, ray.LastHitType);

            // Or visualize the normal: 
            // Vector3D<float> normal = scene.GetNormal(intersectionPoint, ray.LastHitID, ray.LastHitType);
            // return new Vector4D<float>(normal.X, normal.Y, normal.Z, 1f);

            // Or visualize the distance: 
            // return new Vector4D<float>(0.1f * ray.Distance, 0.1f * ray.Distance, 0.1f * ray.Distance, 1f);

            // Or visualize the intersection point:
            // return new Vector4D<float>(
            //     Scalar.Abs(intersectionPoint.X * 0.25f), 
            //     Scalar.Abs(intersectionPoint.Y * 0.25f), 
            //     Scalar.Abs(intersectionPoint.Z * 0.15f), 
            //     1f
            // );
        }
    }
}