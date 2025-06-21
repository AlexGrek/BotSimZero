// See https://aka.ms/new-console-template for more information

using SimuliEngine.MapGen;
using SimuliEngine.Tiles;

Func<TileType, string> mapp = (item) =>
{
    if (item is TileType.Space)
        return "..";
    if (item is TileType.Wall)
        return "##";
    return "xx";
};
ProceduralMapGenerator mapGen = new ProceduralMapGenerator(32, 32);
var map = mapGen.Generate();

if (map == null)
{
    throw new ArgumentNullException(nameof(map), "No map generated");
}

for (int x = 0; x < map.Size.Item1; x++)
{
    for (int y = 0; y < map.Size.Item2; y++)
    {
        Console.Write($"{mapp(map.TileTypeMap[x, y])}");
    }
    Console.WriteLine();
}