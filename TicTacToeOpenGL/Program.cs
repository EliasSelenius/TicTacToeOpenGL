using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Glow;

using OpenTK;

namespace TicTacToeOpenGL {
    class Program : Applet {

        #region statics
        public static Program applet;

        static void Main(string[] args) {
            applet = new Program();
            applet.Start();
        }
        #endregion


        ShaderProgram shader;

        VertexArray vao;
        Buffer<float> vbo;
        Buffer<uint> ebo;

        Texture2D xtexture;
        Texture2D otexture;

        char turn = 'X';
        char[,] board = new char[3, 3] {
            {' ', ' ', ' ' },
            {' ', ' ', ' ' },
            {' ', ' ', ' ' }
        };

        float zoom = 8;

        protected override void Load() {
            shader = CreateShader(File.ReadAllText("data/frag.glsl"), File.ReadAllText("data/vert.glsl"));

            #region init buffers
            vbo = new Buffer<float>();
            vbo.Initialize(new float[] {
                -.5f, -.5f, 1, 0,
                -.5f,  .5f, 0, 0,
                 .5f, -.5f, 1, 1,
                 .5f,  .5f, 0, 1
            }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

            ebo = new Buffer<uint>();
            ebo.Initialize(new uint[] {
                0, 1, 2,
                3, 2, 1
            }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

            vao = new VertexArray();
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, vbo);
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer, ebo);

            vao.AttribPointer(shader.GetAttribLocation("pos"), 2, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            vao.AttribPointer(shader.GetAttribLocation("uv"), 2, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 4, sizeof(float) * 2);
            #endregion

            xtexture = new Texture2D(Resource1.xchar);
            otexture = new Texture2D(Resource1.ochar);

            Window.MouseDown += Window_MouseDown;

        }

        private void Window_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e) {
            var mp = new Vector2((float)e.X / Window.Width, (float)e.Y / Window.Height);
            mp -= Vector2.One * .5f;
            mp.X *= zoom;
            var rez = (float)Window.Height / Window.Width;
            mp.Y *= zoom * rez;

            var x = (int)Math.Floor(Funcs.Map(mp.X, -1.5f, 1.5f, 0, 3));
            var y = (int)Math.Floor(Funcs.Map(mp.Y, -1.5f, 1.5f, 3, 0));
            if (x >= 0 && x < 3 && y >= 0 && y < 3) {
                var i = board[x, y];
                if (i == ' ') {
                    board[x, y] = turn;

                    if (CheckForWin()) {
                        Console.WriteLine(turn + " won the game");
                        ResetGame();
                    }

                    ToggleTurn();
                }
            }
        }

        void ResetGame() {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    board[i, j] = ' ';
                }
            }
        }

        bool CheckForWin() {
            bool isalleq(int x1, int y1, int x2, int y2, int x3, int y3)
                => board[x1, y1] == board[x2, y2] && board[x2, y2] == board[x3, y3] && board[x3, y3] == turn;
            for (int a = 0; a < 3; a++) {
                if (isalleq(a, 0, a, 1, a, 2) || isalleq(0, a, 1, a, 2, a)) {
                    return true;
                }
            }

            return (isalleq(0, 0, 1, 1, 2, 2) || isalleq(2, 0, 1, 1, 0, 2));
        }

        void ToggleTurn() {
            if (turn == 'X') {
                turn = 'O';
            } else {
                turn = 'X';
            }
        }

        protected override void Render() {
            shader.Use();

            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    var e = board[x, y];

                    if (e == 'X') {
                        xtexture.Bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
                    } else if (e == 'O') {
                        otexture.Bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
                    } else {
                        Texture2D.Unbind();
                    }

                    shader.SetVec2("offset", x - 1, y - 1);
                    vao.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 6, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt);
                }
            }

        }

        protected override void Update() {
            
        }

        protected override void OnWindowResize() {
            var rez = (float)Window.Height / Window.Width;
            shader.SetMat4("projection", OpenTK.Matrix4.CreateOrthographic(zoom, zoom * rez, -1, 10));
        }

    }

    static class Funcs {
        public static float Lerp(float t, float min, float max) => min + ((max - min) * t);

        public static float Map(float v, float min, float max, float newmin, float newmax) => Lerp((v - min) / (max - min), newmin, newmax);
    }
}
