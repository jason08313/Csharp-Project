using System;
using System.Linq;

class Program
{
    static void Main()
    {
        // 创建随机数生成器
        Random rand = new Random();

        // 生成 100 个 [0, 1000] 的随机整数
        int[] numbers = new int[100];
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = rand.Next(0, 1001); // 上限为 1001，表示包含 1000
        }

        // 使用 LINQ 排序
        var sortedNumbers = numbers.OrderByDescending(n => n);

        // 求和
        int sum = numbers.Sum();

        // 求平均值
        double average = numbers.Average();

        // 输出排序后的序列
        Console.WriteLine("从大到小排序后的整数序列：");
        foreach (var num in sortedNumbers)
        {
            Console.Write(num + " ");
        }
        Console.WriteLine("\n");

        // 输出总和与平均值
        Console.WriteLine($"总和：{sum}");
        Console.WriteLine($"平均值：{average:F2}"); // 保留两位小数

        Console.ReadKey();
    }
}