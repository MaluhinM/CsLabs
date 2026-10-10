namespace BattleProcess;

public class Program
{
    public static void Main()
    {
        int[][] battles =
        [
            [15, 20, 10],
            [30, 25],
            [40],
            [20, 20, 15, 10],
        ];
        int[] score = new int[battles.Length];
        for (int i = 0; i < battles.Length; i++)
            score[i] = battles[i].Sum();
        float[] mean = new float[battles.Length];
        for (int i = 0; i < battles.Length; i++)
            mean[i] = score[i] / battles[i].Length;
        int maxValue = 0;
        foreach (int[] line in battles)
        {
            if (line.Max() > maxValue)
                maxValue = line.Max();
        }
        Console.WriteLine($"Боец с максимальным количеством очков: {score.IndexOf(score.Max()) + 1}");
        Console.WriteLine($"Боец с максимальным средним арифметическим очков: {mean.IndexOf(mean.Max()) + 1}");
        Console.WriteLine($"Максимальное количество очков: {maxValue}");
    }
}
