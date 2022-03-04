using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolver
{
    public static class Util
    {
        public static RawRectangleF ToSharpDX(this RectangleF rect)
        {
            return new RawRectangleF(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}
