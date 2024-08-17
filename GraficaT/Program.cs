using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Reflection;

namespace GraficaT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "3D T con OpenGL",
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                _window = window;
                window.Load += OnLoad;
                window.RenderFrame += OnRenderFrame;
                window.Run();
            }
        }

        private static float[] vertices = {
            // Parte horizontal
            -0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
             0.5f,  0.7f, -0.5f,  1.0f, 0.0f, 0.0f,
            -0.5f,  0.7f, -0.5f,  1.0f, 0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f,
             0.5f,  0.7f,  0.5f,  1.0f, 0.0f, 0.0f,
            -0.5f,  0.7f,  0.5f,  1.0f, 0.0f, 0.0f,

            // Parte vertical
            -0.1f, -0.5f, -0.1f,  0.0f, 1.0f, 0.0f,
             0.1f, -0.5f, -0.1f,  0.0f, 1.0f, 0.0f,      
             0.1f,  0.4f, -0.1f,  0.0f, 1.0f, 0.0f,
            -0.1f,  0.4f, -0.1f,  0.0f, 1.0f, 0.0f,

            -0.1f, -0.5f,  0.1f,  0.0f, 1.0f, 0.0f,
             0.1f, -0.5f,  0.1f,  0.0f, 1.0f, 0.0f,
             0.1f,  0.4f,  0.1f,  0.0f, 1.0f, 0.0f,
            -0.1f,  0.4f,  0.1f,  0.0f, 1.0f, 0.0f
        };


        private static uint[] indices = {
            // Parte horizontal
            0, 1, 2, 2, 3, 0, // Cara frontal
            4, 5, 6, 6, 7, 4, // Cara trasera
            0, 1, 5, 5, 4, 0, // Parte inferior
            3, 2, 6, 6, 7, 3, // Parte superior
            0, 3, 7, 7, 4, 0, // Lado izquierdo
            1, 2, 6, 6, 5, 1, // Lado derecho

            // Parte vertical
            8, 9, 10, 10, 11, 8, // Cara frontal de la parte vertical
            12, 13, 14, 14, 15, 12, // Cara trasera de la parte vertical
            8, 9, 13, 13, 12, 8, // Parte inferior de la parte vertical
            11, 10, 14, 14, 15, 11, // Parte superior de la parte vertical
            8, 11, 15, 15, 12, 8, // Lado izquierdo de la parte vertical
            9, 10, 14, 14, 13, 9 // Lado derecho de la parte vertical
        };



        private static int shaderProgram;
        private static int vao;
        private static GameWindow _window;

        

        private static void OnLoad()
        {
            // Compila los shaders
            string vertexShaderSource = System.IO.File.ReadAllText("vertexShader.glsl");
            string fragmentShaderSource = System.IO.File.ReadAllText("fragmentShader.glsl");

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Configura VAO y VBO
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Configura atributos de los vértices
            int vertexLocation = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            int colorLocation = GL.GetAttribLocation(shaderProgram, "aColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0); // Desenlazar VAO
        }


        private static void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color4.Black);

            GL.UseProgram(shaderProgram);

            // Matrices de transformación
            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45.0f) * (float)args.Time);
            model = model * Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            Matrix4 view = Matrix4.LookAt(new Vector3(1.5f, 1.5f, 3.0f), Vector3.Zero, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 800f / 600f, 0.1f, 100f);

            int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
            GL.UniformMatrix4(viewLoc, false, ref view);

            int projectionLoc = GL.GetUniformLocation(shaderProgram, "projection");
            GL.UniformMatrix4(projectionLoc, false, ref projection);

            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            // Cambiar buffers
            _window.SwapBuffers();
        }


    }
}



