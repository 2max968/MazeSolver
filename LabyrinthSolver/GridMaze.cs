using System.Collections.Generic;

namespace MazeSolver
{
    public class GridMaze
    {
        public int Width { get;private set; }
        public int Height { get;private set; }
        int[] inputGrid;
        public int[] InputGrid { get => inputGrid; }
        int[] outputGrid;
        public int[] OutputGrid { get => outputGrid; }
        List<(int x, int y)> taskList;
        object taskMutex = new object();

        public List<(int x, int y)> Targets { get; set; } = new List<(int x, int y)>();

        public GridMaze(int w, int h)
            : this(w, h, 1)
        {
            
        }

        public GridMaze(int w, int h, int defaultValue)
        {
            Width = w;
            Height = h;
            inputGrid = new int[w * h];
            outputGrid = new int[w * h];
            for (int i = 0; i < inputGrid.Length; i++)
                inputGrid[i] = defaultValue;
        }

        public bool SetInput(int x, int y, int value)
        {
            try
            {
                inputGrid[ind(x, y)] = value;
                return true;
            }
            catch(System.IndexOutOfRangeException)
            {
                return false;
            }
        }

        public void CalculateOutputs()
        {
            lock(taskMutex)
            {
                for (int i = 0; i < outputGrid.Length; i++)
                    outputGrid[i] = int.MaxValue;
                taskList = new List<(int x, int y)>();
                foreach(var target in Targets)
                {
                    setStep(target.x, target.y, 0);
                    setStep(target.x + 1, target.y, 10);
                    setStep(target.x - 1, target.y, 10);
                    setStep(target.x, target.y + 1, 10);
                    setStep(target.x, target.y - 1, 10);
                }
                while(taskList.Count > 0)
                {
                    step();
                }
            }
        }

        void setStep(int x, int y, int distance)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;
            if (inputGrid[ind(x, y)] < 0)
                return;
            if (outputGrid[ind(x, y)] <= distance)
                return;
            if (!taskList.Contains((x, y)))
                taskList.Add((x, y));
            outputGrid[ind(x, y)] = distance;
        }

        int ind(int x, int y)
        {
            return y * Width + x;
        }
        int ind((int x, int y) position)
        {
            return position.y * Width + position.x;
        }

        void step()
        {
            var tmpList = taskList;
            taskList = new List<(int x, int y)>();
            foreach(var task in tmpList)
            {
                int x = task.x;
                int y = task.y;
                int slowness = inputGrid[ind(x, y)];
                int startVal = outputGrid[ind(x, y)];
                setStep(x + 1, y, 10 * slowness + startVal);
                setStep(x - 1, y, 10 * slowness + startVal);
                setStep(x, y + 1, 10 * slowness + startVal);
                setStep(x, y - 1, 10 * slowness + startVal);
            }
        }

        public int GetOutput(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return int.MaxValue;
            return outputGrid[ind(x, y)];
        }

        public int GetInput(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return int.MaxValue;
            return inputGrid[ind(x, y)];
        }

        (int x, int y) getBestDirection(int x, int y)
        {
            var directions = new (int x, int y)[]
            {
                (1,0), (-1,0), (0,1), (0,-1),
                (1,1), (1,-1), (-1,-1), (-1,1)
            };

            int minDir = 0;
            int minVal = GetOutput(x + directions[0].x, y + directions[0].y);
            for(int i = 1; i < directions.Length; i++)
            {
                int cval = GetOutput(x, y);
                if (directions[i].x != 0 && directions[i].y != 0)
                {
                    var p1 = (x + directions[i].x, y);
                    var p2 = (x, y + directions[i].y);
                    var val1 = GetOutput(p1.Item1, p1.Item2);
                    var val2 = GetOutput(p2.Item1, p2.Item2);
                    if (val1 > cval || val2 > cval)
                        continue;
                }
                (int x, int y) pos = (x + directions[i].x, y + directions[i].y);
                int val = GetOutput(pos.x, pos.y);
                if (val < minVal)
                {
                    minDir = i;
                    minVal = val;
                }
            }
            if (minVal >= GetOutput(x, y))
                return (0, 0);
            return directions[minDir];
        }

        public List<(int x, int y)> GetPath(int startX, int startY)
        {
            var path = new List<(int x, int y)>();
            (int x, int y) pos = (startX, startY);
            path.Add(pos);
            while(outputGrid[ind(pos)] > 0)
            {
                var dir = getBestDirection(pos.x, pos.y);
                if (dir.x == 0 && dir.y == 0)
                    return null;
                pos.x += dir.x;
                pos.y += dir.y;
                path.Add(pos);
            }
            return path;
        }

        public (int min, int max) GetMinAndMaxOutput()
        {
            int min = outputGrid[0];
            int max = outputGrid[0];
            for(int i = 1; i < outputGrid.Length; i++)
            {
                if (outputGrid[i] == int.MaxValue) continue;
                if (outputGrid[i] < min) min = outputGrid[i];
                if (outputGrid[i] > max) max = outputGrid[i];
            }
            return (min, max);
        }
    }
}
