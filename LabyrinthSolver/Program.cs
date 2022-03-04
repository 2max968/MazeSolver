using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeSolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());

            GridMaze maze = new GridMaze(6, 6);
            for(int i = 0; i < 5; i++)
            {
                maze.SetInput(2, i, -1);
                maze.SetInput(4, 5 - i, -1);
            }
            maze.SetInput(4, 3, 1);
            maze.Targets.Add((0, 0));
            maze.CalculateOutputs();
            
            for(int y = 0; y < maze.Height; y++)
            {
                for(int x = 0; x < maze.Width; x++)
                {
                    Console.Write($"{maze.GetOutput(x,y), 8}");
                }
                Console.WriteLine();
            }
            var path = maze.GetPath(5, 5);
            return;
        }
    }
}
