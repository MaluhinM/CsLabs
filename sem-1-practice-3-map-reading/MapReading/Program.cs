namespace MapReading;

public class Program
{
    public static int[][] GetMatrics(string[] lines)
    {
        int[][] result = new int[lines.Length][];
        for (int i = 0; i < lines.Length; i++)
        {
            result[i] = lines[i].Split([' ', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
        }
        return result;
    }

    public static uint CountInMatrics(int[][] matrics, int element)
    {
        uint result = 0;
        foreach (int[] line in matrics)
            result += (uint)line.Count(element);
        return result;
    }

    public static uint GetMaxLineInMatrics(int[][] matrics, int element)
    {
        uint maxCount = 0;
        uint result = 0;
        for (uint i = 0; i < matrics.Length; i++)
            {
                uint count = (uint)matrics[i].Count(element);
                if (count > maxCount)
                {
                    maxCount = count;
                    result = i;
                }
            }
        return result;
    }

    public static void Main()
    {
        string[] lines = File.ReadAllLines("map.txt");
        int[][] map = GetMatrics(lines);
        Console.WriteLine($"Всего сокровищ: {CountInMatrics(map, 2)}");
        Console.WriteLine($"Всего ловушек: {CountInMatrics(map, -1)}");
        Console.WriteLine($"Больше всего сокровищ в строке № {GetMaxLineInMatrics(map, 2) + 1}");
    }
}
