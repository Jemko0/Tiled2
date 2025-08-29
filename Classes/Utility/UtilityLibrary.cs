using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2.Utility
{
    public class UtilityLibrary
    {
        public static bool IsValidIndex(Array a, int[] indices)
        {
            for(int dimension = 0; dimension < indices.Length; dimension++)
            {
                if (indices[dimension] < 0)
                {
                    return false;
                }

                int currLen = a.GetLength(dimension);

                if (indices[dimension] >= currLen)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
