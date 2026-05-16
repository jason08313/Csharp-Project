using System;
using System.Collections.Generic;

// 数组分析工具类：封装所有计算逻辑
public class IntArrayStats
{
    private readonly int[] numbers;
    
    public IntArrayStats(int[] numbers)
    {
        if (numbers == null || numbers.Length == 0)
            throw new ArgumentException("数组不能为空。");
        
        this.numbers = numbers;
    }

    public int GetMax()
    {
        int max = numbers[0];
        foreach (int num in numbers)
        {
            if (num > max)
                max = num;
        }
        return max;
    }

    public int GetMin()
    {
        int min = numbers[0];
        foreach (int num in numbers)
        {
            if (num < min)
                min = num;
        }
        return min;
    }

    public long GetSum()
    {
        long sum = 0;
        foreach (int num in numbers)
        {
            sum += num;
        }
        return sum;
    }

    public double GetAverage()
    {
        return (double)GetSum() / numbers.Length;
    }
}

// 主程序：测试类的方法
class Program
{
    static void Main()
    {
        // 测试用整数数组
        int[] testArray;
        Console.WriteLine("请输入整数数组（用逗号分隔）：");
        string input = Console.ReadLine();

        // 将输入的字符串转换为整数数组
        testArray = Array.ConvertAll(input.Split(','), int.Parse);

        try
        {
            var stats = new IntArrayStats(testArray);
            Console.WriteLine($"最大值: {stats.GetMax()}");
            Console.WriteLine($"最小值: {stats.GetMin()}");
            Console.WriteLine($"和: {stats.GetSum()}");
            Console.WriteLine($"平均值: {stats.GetAverage()}");
        }
        catch (ArgumentException ex)
        {
            // 捕获异常并提示
            Console.WriteLine(ex.Message);
        }
    }
}