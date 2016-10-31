using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanvasToScript.Common
{
    class MatrixTransformations
    {
        public static T[,] ResizeArray<T>(ref T[,] original, int fromRow, int fromCol, int toRow, int toCol)
        {
            T[,] _arr = new T[toRow - fromRow + 1, toCol - fromCol + 1];

            int r = 0;
            for (int i = fromRow; i <= toRow; i++)
            {
                int c = 0;
                for (int j = fromCol; j <= toCol; j++)
                {
                    _arr[r, c] = original[i, j];
                    c++;
                }
                r++;
            }

            return _arr;
        }
    }
}
