using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Basic
{
    public class SubdivisionCell<T>: IDumpable where T : class
    {
        private T[,]? _data; // Internal 2D array, null if all cells are null
        private int _nonNullCount; // Tracks the number of non-null entries

        public int Subdivisions { get; }

        public int NonNullCount => _nonNullCount;

        public bool IsSubdivided => _data != null; // True if the internal array is initialized

        public SubdivisionCell(int subdivisions)
        {
            if (subdivisions <= 0)
                throw new ArgumentOutOfRangeException(nameof(subdivisions), "Subdivisions must be greater than zero.");

            Subdivisions = subdivisions;
            _data = null; // Start with no data
            _nonNullCount = 0;
        }

        public T? this[int row, int column]
        {
            get
            {
                ValidateIndices(row, column);
                return _data?[row, column];
            }
            set
            {
                ValidateIndices(row, column);

                if (_data == null && value != null)
                {
                    // Initialize the array when the first non-null value is set
                    _data = new T[Subdivisions, Subdivisions];
                }

                if (_data != null)
                {
                    if (_data[row, column] == null && value != null)
                    {
                        _nonNullCount++;
                    }
                    else if (_data[row, column] != null && value == null)
                    {
                        _nonNullCount--;
                    }

                    _data[row, column] = value;

                    // If all cells are null, release the array
                    if (_nonNullCount == 0)
                    {
                        _data = null;
                    }
                }
            }
        }

        public bool IsAllNull => _data == null;

        private void ValidateIndices(int row, int column)
        {
            if (row < 0 || row >= Subdivisions || column < 0 || column >= Subdivisions)
                throw new IndexOutOfRangeException("Row or column index is out of range.");
        }
    }
}
