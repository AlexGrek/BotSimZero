using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Basic
{
    /// <summary>
    /// Base class for hypercells containing a 2D array of elements
    /// </summary>
    public abstract class HypercellBase<T>: IDumpable
    {
        protected readonly int size;
        protected readonly (int x, int y) coordinates;

        public (int x, int y) Coordinates => coordinates;
        public int Size => size;

        protected HypercellBase(int size, (int x, int y) coordinates)
        {
            this.size = size;
            this.coordinates = coordinates;
        }

        public abstract T this[int x, int y] { get; set; }

        public IEnumerable<T> AllLeft()
        {
            for (int y = 0; y < size; y++)
            {
                yield return this[0, y];
            }
        }

        public IEnumerable<T> AllRight()
        {
            for (int y = 0; y < size; y++)
            {
                yield return this[size - 1, y];
            }
        }

        public IEnumerable<T> AllTop()
        {
            for (int x = 0; x < size; x++)
            {
                yield return this[x, 0];
            }
        }

        public IEnumerable<T> AllBottom()
        {
            for (int x = 0; x < size; x++)
            {
                yield return this[x, size - 1];
            }
        }

        public IEnumerable<T> AllCells()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    yield return this[x, y];
                }
            }
        }

        public IEnumerable<(int x, int y, T value)> AllCellsWithCoordinates()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    yield return (x, y, this[x, y]);
                }
            }
        }
    }

    /// <summary>
    /// Single-buffered implementation of Hypercell
    /// </summary>
    public class Hypercell<T> : HypercellBase<T>
    {
        protected readonly T[,] cells;

        public void Fill(T value)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    cells[x, y] = value;
                }
            }
        }

        public void Fill(Func<T> value)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    cells[x, y] = value();
                }
            }
        }

        public Hypercell(int size, (int x, int y) coordinates) : base(size, coordinates)
        {
            cells = new T[size, size];
        }

        public override T this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= size || y < 0 || y >= size)
                    throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) out of range for hypercell of size {size}");
                return cells[x, y];
            }
            set
            {
                if (x < 0 || x >= size || y < 0 || y >= size)
                    throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) out of range for hypercell of size {size}");
                cells[x, y] = value;
            }
        }
    }

    /// <summary>
    /// Double-buffered implementation of Hypercell with buffer swapping
    /// </summary>
    public class DoubleBufferedHypercell<T> : HypercellBase<T>
    {
        protected T[,] currentBuffer;
        protected T[,] nextBuffer;
        protected bool isProcessing = false;

        public void FillIgnoringBuffer(T value)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    currentBuffer[x, y] = value;
                }
            }
        }

        public void FillNextBuffer(T value)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    nextBuffer[x, y] = value;
                }
            }
        }

        public DoubleBufferedHypercell(int size, (int x, int y) coordinates) : base(size, coordinates)
        {
            currentBuffer = new T[size, size];
            nextBuffer = new T[size, size];
        }

        public override T this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= size || y < 0 || y >= size)
                    throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) out of range for hypercell of size {size}");

                // Always read from current buffer
                return currentBuffer[x, y];
            }
            set
            {
                if (x < 0 || x >= size || y < 0 || y >= size)
                    throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) out of range for hypercell of size {size}");

                // Write to next buffer when processing, or to current buffer when not
                if (isProcessing)
                    nextBuffer[x, y] = value;
                else
                    currentBuffer[x, y] = value;
            }
        }

        /// <summary>
        /// Start processing mode - all writes will go to next buffer
        /// </summary>
        public void BeginProcessing()
        {
            isProcessing = true;
        }

        /// <summary>
        /// Write to next buffer even outside of processing mode
        /// </summary>
        public void WriteToNextBuffer(int x, int y, T value)
        {
            if (x < 0 || x >= size || y < 0 || y >= size)
                throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) out of range for hypercell of size {size}");

            nextBuffer[x, y] = value;
        }

        /// <summary>
        /// Finalize processing by swapping buffers
        /// </summary>
        public void FinalizeProcessing()
        {
            var temp = currentBuffer;
            currentBuffer = nextBuffer;
            nextBuffer = temp;
            isProcessing = false;
        }
    }

    /// <summary>
    /// Base implementation of a hypermap that manages hypercells
    /// </summary>
    public abstract class HyperMapBase<TCellType, THypercellType> where THypercellType : HypercellBase<TCellType>, IDumpable
    {
        protected readonly int hypercellSize;
        protected readonly ConcurrentDictionary<(int, int), THypercellType> hypercells;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public HyperMapBase(int hypercellSize)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this.hypercellSize = hypercellSize;
            hypercells = new ConcurrentDictionary<(int, int), THypercellType>();
        }

        public HyperMapBase<TCellType, THypercellType> PreInit((int, int) Size)
        {
            int hypercellCountX = (int)Math.Ceiling((double)Size.Item1 / hypercellSize);
            int hypercellCountY = (int)Math.Ceiling((double)Size.Item2 / hypercellSize);
            for (int x = 0; x < hypercellCountX; x++)
            {
                for (int y = 0; y < hypercellCountY; y++)
                {
                    var coordinates = (x, y);
                    hypercells.TryAdd(coordinates, CreateHypercell(coordinates));
                }
            }
            return this; // Return the current instance for chaining
        }

        protected abstract THypercellType CreateHypercell((int x, int y) coordinates);

        protected (int, int) GetHypercellCoordinates(int x, int y)
        {
            return (
                (int)Math.Floor((double)x / hypercellSize),
                (int)Math.Floor((double)y / hypercellSize)
            );
        }

        protected (int, int) GetLocalCellCoordinates(int x, int y)
        {
            int localX = x >= 0
                ? x % hypercellSize
                : (x % hypercellSize + hypercellSize) % hypercellSize;

            int localY = y >= 0
                ? y % hypercellSize
                : (y % hypercellSize + hypercellSize) % hypercellSize;

            return (localX, localY);
        }

        // This indexer accesses individual cells using global coordinates
        public TCellType this[int x, int y]
        {
            get
            {
                var hyperCoords = GetHypercellCoordinates(x, y);
                var localCoords = GetLocalCellCoordinates(x, y);

                if (!hypercells.TryGetValue(hyperCoords, out var hypercell))
                    throw new KeyNotFoundException($"Hypercell at coordinates {hyperCoords} not found");

                return hypercell[localCoords.Item1, localCoords.Item2];
            }
            set
            {
                var hyperCoords = GetHypercellCoordinates(x, y);
                var localCoords = GetLocalCellCoordinates(x, y);

                var hypercell = hypercells.GetOrAdd(hyperCoords, CreateHypercell);
                hypercell[localCoords.Item1, localCoords.Item2] = value;
            }
        }

        // Property to access hypercells by their coordinates
        public class HypercellAccessor
        {
            private readonly HyperMapBase<TCellType, THypercellType> parent;

            public HypercellAccessor(HyperMapBase<TCellType, THypercellType> parent)
            {
                this.parent = parent;
            }

            public THypercellType this[int hyperX, int hyperY]
            {
#pragma warning disable CS8603 // Possible null reference return.
                get => parent.hypercells.TryGetValue((hyperX, hyperY), out THypercellType? cell) ? cell : null;
#pragma warning restore CS8603 // Possible null reference return.
                set
                {
                    if (value == null)
                        parent.hypercells.TryRemove((hyperX, hyperY), out _);
                    else
                        parent.hypercells[(hyperX, hyperY)] = value;
                }
            }
        }

        // Property that returns the hypercell accessor
        private HypercellAccessor _hypercellAccessor;
        public HypercellAccessor Hypercell => _hypercellAccessor ??= new HypercellAccessor(this);

        public IEnumerable<THypercellType> AllHypercells()
        {
            return hypercells.Values;
        }

        public bool TryGetHypercell((int x, int y) hypercellCoords, out THypercellType hypercell)
        {
            return hypercells.TryGetValue(hypercellCoords, out hypercell);
        }

        public bool TryRemoveHypercell((int x, int y) hypercellCoords, out THypercellType removedHypercell)
        {
            return hypercells.TryRemove(hypercellCoords, out removedHypercell);
        }
    }

    /// <summary>
    /// HyperMap implementation with single-buffered hypercells
    /// </summary>
    public class HyperMap<T> : HyperMapBase<T, Hypercell<T>>
    {
        public HyperMap(int hypercellSize) : base(hypercellSize) { }

        public void Fill(T baseValue)
        {
            foreach (var hypercell in hypercells.Values)
            {
                for (int y = 0; y < hypercellSize; y++)
                {
                    for (int x = 0; x < hypercellSize; x++)
                    {
                        hypercell.Fill(baseValue);
                    }
                }
            }
        }

        public void Fill(Func<T> baseValueProducer)
        {
            foreach (var hypercell in hypercells.Values)
            {
                for (int y = 0; y < hypercellSize; y++)
                {
                    for (int x = 0; x < hypercellSize; x++)
                    {
                        hypercell.Fill(baseValueProducer);
                    }
                }
            }
        }

        protected override Hypercell<T> CreateHypercell((int x, int y) coordinates)
        {
            return new Hypercell<T>(hypercellSize, coordinates);
        }

        public Hypercell<T> GetOrCreateHypercell(int hyperX, int hyperY)
        {
            return hypercells.GetOrAdd((hyperX, hyperY), CreateHypercell);
        }

        public void ProcessAllCellsInParallel(Action<int, int, T> processAction)
        {
            Parallel.ForEach(hypercells, hypercellEntry =>
            {
                var hypercell = hypercellEntry.Value;
                var baseX = hypercell.Coordinates.x * hypercellSize;
                var baseY = hypercell.Coordinates.y * hypercellSize;

                for (int y = 0; y < hypercellSize; y++)
                {
                    for (int x = 0; x < hypercellSize; x++)
                    {
                        processAction(baseX + x, baseY + y, hypercell[x, y]);
                    }
                }
            });
        }
    }

    /// <summary>
    /// HyperMap implementation with double-buffered hypercells
    /// </summary>
    public class DoubleBufferedHyperMap<T> : HyperMapBase<T, DoubleBufferedHypercell<T>>
    {
        public DoubleBufferedHyperMap(int hypercellSize) : base(hypercellSize) { }

        protected override DoubleBufferedHypercell<T> CreateHypercell((int x, int y) coordinates)
        {
            return new DoubleBufferedHypercell<T>(hypercellSize, coordinates);
        }

        public DoubleBufferedHypercell<T> GetOrCreateHypercell(int hyperX, int hyperY)
        {
            return hypercells.GetOrAdd((hyperX, hyperY), CreateHypercell);
        }

        public void BeginProcessing()
        {
            foreach (var hypercell in hypercells.Values)
            {
                hypercell.BeginProcessing();
            }
        }

        public void FinalizeProcessing()
        {
            foreach (var hypercell in hypercells.Values)
            {
                hypercell.FinalizeProcessing();
            }
        }

        public void ProcessAllCellsInParallel(Func<int, int, T, T> updateFunction)
        {
            BeginProcessing();

            Parallel.ForEach(hypercells, hypercellEntry =>
            {
                var hypercell = hypercellEntry.Value;
                var baseX = hypercell.Coordinates.x * hypercellSize;
                var baseY = hypercell.Coordinates.y * hypercellSize;

                for (int y = 0; y < hypercellSize; y++)
                {
                    for (int x = 0; x < hypercellSize; x++)
                    {
                        var newValue = updateFunction(baseX + x, baseY + y, hypercell[x, y]);
                        hypercell.WriteToNextBuffer(x, y, newValue);
                    }
                }
            });

            FinalizeProcessing();
        }

        public void ProcessAllCellsInParallel(Func<int, int, DoubleBufferedHyperMap<T>, T, T> updateFunction)
        {
            BeginProcessing();

            Parallel.ForEach(hypercells, hypercellEntry =>
            {
                var hypercell = hypercellEntry.Value;
                var baseX = hypercell.Coordinates.x * hypercellSize;
                var baseY = hypercell.Coordinates.y * hypercellSize;

                for (int y = 0; y < hypercellSize; y++)
                {
                    for (int x = 0; x < hypercellSize; x++)
                    {
                        var newValue = updateFunction(baseX + x, baseY + y, this, hypercell[x, y]);
                        hypercell.WriteToNextBuffer(x, y, newValue);
                    }
                }
            });

            FinalizeProcessing();
        }

        public void FillIgnoringBuffer(T baseValue)
        {
            foreach (var hypercell in hypercells.Values)
            {
                hypercell.FillIgnoringBuffer(baseValue);
            }
        }
    }
}
