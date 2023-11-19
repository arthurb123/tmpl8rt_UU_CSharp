using System.Drawing;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

using SilkIWindow = Silk.NET.Windowing.IWindow;
using SilkWindow = Silk.NET.Windowing.Window;

namespace tmpl8rt_UU_CSharp {
    public class Window
    {
        public static Window? Instance { get; private set; }  

        private SilkIWindow? _window;
        private Scene? _scene;
        private Camera? _camera;
        private unsafe void* _bufferPtr;

        private int _fps = 0;
        private int _frames = 0;
        private float _elapsedTime = 0f;

        private uint _vbo;
        private uint _ebo;
        private uint _vao;
        private uint _shader;
        private uint _texture;

        //Vertex shaders are run on each vertex.
        private readonly string _vertexShaderSource = @"
            #version 330 core
            
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec2 aTexCoords;

            out vec2 frag_texCoords;
            
            void main()
            {
                gl_Position = vec4(aPosition, 1.0);

                frag_texCoords = aTexCoords;
            }";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private readonly string _fragmentShaderSource = @"
            #version 330 core

            in vec2 frag_texCoords;
            out vec4 out_color;
            uniform sampler2D uTexture;

            void main()
            {
                out_color = texture(uTexture, frag_texCoords);
            }";

        //Vertex data, uploaded to the VBO.
        private readonly float[] _vertices =
        {
            //X   Y    Z      TexCoords
             1f,  -1f, 0.0f,  1.0f, 1.0f,
             1f,   1f, 0.0f,  1.0f, 0.0f,
            -1f,   1f, 0.0f,  0.0f, 0.0f,
            -1f,  -1f, 0.0f,  0.0f, 1.0f
        };

        //Index data, uploaded to the EBO.
        private readonly uint[] _indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };


        public unsafe Window(string title, int width, int height) {
            // Check if an instance already exists
            if (Instance != null)
                throw new System.Exception("Only one instance of Window is allowed!");

            // Create new instance
            Instance = this;

            // Create a Silk.NET window as usual
            _window = SilkWindow.Create(
                new WindowOptions() {
                    Title = title,
                    Size = new Vector2D<int>(width, height),
                    Position = new Vector2D<int>(50, 50),
                    API = GraphicsAPI.Default,
                    ShouldSwapAutomatically = true,
                    WindowBorder = WindowBorder.Fixed,
                    WindowState = WindowState.Normal,
                    VSync = true,
                    IsVisible = true
                }
            );

            // Declare some variables
            ImGuiController? controller = null;
            GL? gl = null;
            IInputContext? inputContext = null;

            // Create our scene and camera
            _scene = new Scene();
            _camera = new Camera();
            _bufferPtr = _camera.AllocateBuffer();

            // Our loading function
            _window.Load += () =>
            {
                // Create our ImGui controller
                controller = new ImGuiController(
                    gl = _window.CreateOpenGL(), // load OpenGL
                    _window, // pass in our window
                    inputContext = _window.CreateInput() // create an input context
                );

                // Create a vertex array.
                _vao = gl.GenVertexArray();
                gl.BindVertexArray(_vao);

                //Initializing a vertex buffer that holds the vertex data.
                _vbo = gl.GenBuffer(); //Creating the buffer.
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo); //Binding the buffer.
                fixed (void* v = &_vertices[0])
                {
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (_vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw); //Setting buffer data.
                }

                //Initializing a element buffer that holds the index data.
                _ebo = gl.GenBuffer(); //Creating the buffer.
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo); //Binding the buffer.
                fixed (void* i = &_indices[0])
                {
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (_indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw); //Setting buffer data.
                }

                //Creating a vertex shader.
                uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
                gl.ShaderSource(vertexShader, _vertexShaderSource);
                gl.CompileShader(vertexShader);

                //Checking the shader for compilation errors.
                string infoLog = gl.GetShaderInfoLog(vertexShader);
                if (!string.IsNullOrWhiteSpace(infoLog))
                {
                    Console.WriteLine($"Error compiling vertex shader {infoLog}");
                }

                //Creating a fragment shader.
                uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
                gl.ShaderSource(fragmentShader, _fragmentShaderSource);
                gl.CompileShader(fragmentShader);

                //Checking the shader for compilation errors.
                infoLog = gl.GetShaderInfoLog(fragmentShader);
                if (!string.IsNullOrWhiteSpace(infoLog))
                {
                    Console.WriteLine($"Error compiling fragment shader {infoLog}");
                }

                //Combining the shaders under one shader program.
                _shader = gl.CreateProgram();
                gl.AttachShader(_shader, vertexShader);
                gl.AttachShader(_shader, fragmentShader);
                gl.LinkProgram(_shader);

                //Checking the linking for errors.
                gl.GetProgram(_shader, GLEnum.LinkStatus, out var status);
                if (status == 0)
                {
                    Console.WriteLine($"Error linking shader {gl.GetProgramInfoLog(_shader)}");
                }

                //Delete the no longer useful individual shaders;
                gl.DetachShader(_shader, vertexShader);
                gl.DetachShader(_shader, fragmentShader);
                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);

                // Set up our vertex attributes! These tell the vertex array (VAO) how to process the vertex data we defined
                // earlier. Each vertex array contains attributes. 

                // Our stride constant. The stride must be in bytes, so we take the first attribute (a vec3), multiply it
                // by the size in bytes of a float, and then take our second attribute (a vec2), and do the same.
                const uint stride = (3 * sizeof(float)) + (2 * sizeof(float));

                // Enable the "aPosition" attribute in our vertex array, providing its size and stride too.
                const uint positionLoc = 0;
                gl.EnableVertexAttribArray(positionLoc);
                gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*) 0);

                // Now we need to enable our texture coordinates! We've defined that as location 1 so that's what we'll use
                // here. The code is very similar to above, but you must make sure you set its offset to the **size in bytes**
                // of the attribute before.
                const uint textureLoc = 1;
                gl.EnableVertexAttribArray(textureLoc);
                gl.VertexAttribPointer(textureLoc, 2, VertexAttribPointerType.Float, false, stride, (void*) (3 * sizeof(float)));

                // Unbind everything as we don't need it.
                gl.BindVertexArray(0);
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

                // Now we create our texture!
                _texture = gl.GenTexture();

                // Much like our texture creation earlier, we must first set our active texture unit, and then bind the
                // texture to use it during draw!
                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(TextureTarget.Texture2D, _texture);

                // Upload the buffer to the GPU, this won't do much but we do this
                // so that we can set the texture parameters :)
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f, (uint)Camera.WIDTH, 
                    (uint)Camera.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, _bufferPtr);

                // Let's set some texture parameters!
                
                // Set the texture wrap mode to repeat.
                gl.TextureParameter(_texture, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                gl.TextureParameter(_texture, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

                // The min and mag filters define how the texture should be sampled as it resized.
                gl.TextureParameter(_texture, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.LinearMipmapLinear);
                gl.TextureParameter(_texture, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

                // Generate mipmaps for this texture.
                gl.GenerateMipmap(TextureTarget.Texture2D);

                // Get our texture uniform, and set it to the texture location.
                int location = gl.GetUniformLocation(_shader, "uTexture");
                if (location != -1)
                    gl.Uniform1(location, _texture);

                // Blending setup remains the same
                gl?.Enable(EnableCap.Blend);
                gl?.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                // Enable multisampling
                gl?.Enable(EnableCap.Multisample);
            };

            // Handle resizes
            _window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                gl?.Viewport(s);
            };

            // The update function
            _window.Update += delta => {
                // Update scene
                _scene.SetTime(_scene.AnimationTime + (float)delta * 0.2f);

                // Handle camera input
                var keyboard = inputContext?.Keyboards[0];
                if (keyboard != null)
                    _camera.HandleInput(keyboard, (float)delta);

                // If the user pressed the escape
                // button close the window
                if (keyboard?.IsKeyPressed(Key.Escape) ?? false)
                    _window.Close();
            };

            // The render function
            _window.Render += delta =>
            {
                // Make sure ImGui is up-to-date
                controller?.Update((float)delta);

                // Clear the screen
                gl?.ClearColor(System.Drawing.Color.FromArgb(255, (int)(.45f * 255), (int)(.55f * 255), (int)(.60f * 255)));
                gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

                // Render the scene
                Renderer.RenderScene(_scene, _camera, _bufferPtr);

                // Activate and bind the texture
                gl?.ActiveTexture(TextureUnit.Texture0);
                gl?.BindTexture(TextureTarget.Texture2D, _texture);
                
                // Upload the buffer to the GPU
                gl?.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba16f, (uint) Camera.WIDTH, 
                    (uint) Camera.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, _bufferPtr);

                // Bind the geometry and shader and draw the quad, consisting of 2 triangles (6 vertices)
                gl?.BindVertexArray(_vao);
                gl?.UseProgram(_shader);
                gl?.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);

                // Let ImGUI show an FPS counter
                _elapsedTime += (float)delta;
                if (_elapsedTime >= 1f)
                {
                    _fps = _frames;
                    _frames = 0;
                    _elapsedTime = 0f;
                }
                ImGuiNET.ImGui.Text($"FPS: {_fps}");
                ImGuiNET.ImGui.Text($"Frame time: {delta * 1000f}ms");

                // Make sure ImGui renders too!
                controller?.Render();

                // Add frame
                _frames++;
            };

            // The closing function
            _window.Closing += () =>
            {
                // Dispose our buffer
                Marshal.FreeHGlobal((IntPtr)_bufferPtr);

                // Dispose scene
                _scene?.Dispose();

                // Dispose our controller first
                controller?.Dispose();

                // Dispose the input context
                inputContext?.Dispose();

                // Dispose GL objects
                gl?.DeleteBuffer(_vbo);
                gl?.DeleteBuffer(_ebo);
                gl?.DeleteTexture(_texture);
                gl?.DeleteVertexArray(_vao);
                gl?.DeleteProgram(_shader);

                // Unload OpenGL
                gl?.Dispose();
            };

            // Now that everything's defined, let's run this bad boy!
            _window.Run();

            // Call dispose manually to clean up the window
            _window.Dispose();
        }
    }
}