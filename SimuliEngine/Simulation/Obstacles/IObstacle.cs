namespace SimuliEngine.Simulation.Obstacles
{
    public interface IObstacle
    {
        public float HalfSize { get; }
        public (int x, int y) MainPosition { get; }

        public bool IsTemporary { get; }

        public bool IsMoving { get; }
    }
}