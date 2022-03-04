using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeSolver
{
    public partial class Form1 : Form
    {
        GridMaze maze;
        enum EditMode
        {
            Path,
            Wall
        };

        EditMode editMode = EditMode.Wall;

        Factory factory;
        WindowRenderTarget rt;
        SolidColorBrush brushBlack;
        SolidColorBrush brushCyan;
        SolidColorBrush brushWhite;

        public Form1()
        {
            InitializeComponent();
            maze = new GridMaze(40, 40, 3);
            maze.Targets.Add((maze.Width - 1, maze.Height - 1));

            factory = new Factory();
            rt = new WindowRenderTarget(factory, new RenderTargetProperties() { }, new HwndRenderTargetProperties()
            {
                PixelSize = new SharpDX.Size2(pictureBox1.Width, pictureBox1.Height),
                Hwnd = pictureBox1.Handle,
                PresentOptions = PresentOptions.Immediately
            });
            brushBlack = new SolidColorBrush(rt, new RawColor4(0, 0, 0, 1));
            brushCyan = new SolidColorBrush(rt, new RawColor4(0, 1, 1, 1));
            brushWhite = new SolidColorBrush(rt, new RawColor4(1, 1, 1, 1));

            renderMaze();

            this.Disposed += Form1_Disposed;
        }

        private void Form1_Disposed(object sender, EventArgs e)
        {
            brushWhite.Dispose();
            brushCyan.Dispose();
            brushBlack.Dispose();
            rt.Dispose();
            factory.Dispose();
        }

        void renderMaze()
        {
            Stopwatch stp = new Stopwatch();
            stp.Start();
            maze.CalculateOutputs();
            stp.Stop();
            this.Text = "calctime: " + stp.ElapsedMilliseconds + " ms";

            int h = maze.Height * 16;
            int w = maze.Width * 16;
            rt.Resize(new SharpDX.Size2(pictureBox1.Width, pictureBox1.Height));
            rt.BeginDraw();
            rt.Clear(new RawColor4(1, 1, 1, 1));
            for (int i = 0; i < maze.Width; i++)
                rt.DrawLine(new RawVector2(i * 16, 0), new RawVector2(i * 16, h), brushBlack);
            for (int i = 0; i < maze.Height; i++)
                rt.DrawLine(new RawVector2(0, i * 16), new RawVector2(w, i * 16), brushBlack);

            Font ft = new Font("Calibri", 8);
            var bounds = maze.GetMinAndMaxOutput();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int val = maze.GetInput(x, y);
                    var rect = new RectangleF(x * 16, y * 16, 16, 16).ToSharpDX();
                    if (val < 0)
                        rt.FillRectangle(rect, brushBlack);
                    else if (val == 1)
                        rt.FillRectangle(rect, brushCyan);
                    else
                    {
                        int oVal = maze.GetOutput(x, y);
                        float relVal = (oVal - bounds.min) / (float)(bounds.max - bounds.min);
                        RawColor4 c;
                        if (relVal > .5 && relVal <= 1)
                            c = new RawColor4(1, (1 - relVal) * 2, 0, 1);
                        else if (relVal <= .5 && relVal > 0)
                            c = new RawColor4(relVal * 2, 1, 0, 1);
                        else
                            continue;
                        using (var b = new SolidColorBrush(rt, c))
                            rt.FillRectangle(rect, b);
                    }
                }
            }
            ft.Dispose();

            var path = maze.GetPath(0, 0);
            if (path != null)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    RawVector2 p1 = new RawVector2(path[i - 1].x * 16 + 8, path[i - 1].y * 16 + 8);
                    RawVector2 p2 = new RawVector2(path[i].x * 16 + 8, path[i].y * 16 + 8);
                    rt.DrawLine(p1, p2, brushWhite, 3);
                }
            }
            rt.EndDraw();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            pictureBox1_MouseMove(sender, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X / 16;
            int y = e.Y / 16;
            if (e.Button == MouseButtons.Left && maze.GetInput(x,y) != -1 && editMode == EditMode.Wall)
            {
                maze.SetInput(x, y, -1);
                renderMaze();
            }
            else if(e.Button == MouseButtons.Left && maze.GetInput(x,y) != 1 && editMode == EditMode.Path)
            {
                maze.SetInput(x, y, 1);
                renderMaze();
            }
            else if (e.Button == MouseButtons.Right && maze.GetInput(x,y) != 3)
            {
                maze.SetInput(x, y, 3);
                renderMaze();
            }

            lblTileInfo.Text = $"Tile: slowness: {maze.GetInput(x, y)}; distance: {maze.GetOutput(x, y)}";
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            editMode = EditMode.Path;
        }

        private void btnWall_Click(object sender, EventArgs e)
        {
            editMode = EditMode.Wall;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            renderMaze();
        }

        private void pictureBox1_MouseClick_1(object sender, MouseEventArgs e)
        {
            pictureBox1_MouseMove(sender, e);
        }

        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            pictureBox1_MouseMove(sender, e);
        }
    }
}
