using System;

// 数值类型信息查询程序
class Program
{
    static void Main()
    {
        // ===================== 表头（文本对齐，美观清晰） =====================
        // 格式说明：{索引, 宽度} 负数=左对齐，正数=右对齐
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ 数据类型     占用字节数      最小值                         最大值                         ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════════╣");

        PrintTypeInformation("sbyte", sizeof(sbyte), sbyte.MinValue, sbyte.MaxValue);
        PrintTypeInformation("byte", sizeof(byte), byte.MinValue, byte.MaxValue);
        PrintTypeInformation("short", sizeof(short), short.MinValue, short.MaxValue);
        PrintTypeInformation("ushort", sizeof(ushort), ushort.MinValue, ushort.MaxValue);
        PrintTypeInformation("int", sizeof(int), int.MinValue, int.MaxValue);
        PrintTypeInformation("uint", sizeof(uint), uint.MinValue, uint.MaxValue);
        PrintTypeInformation("long", sizeof(long), long.MinValue, long.MaxValue);
        PrintTypeInformation("ulong", sizeof(ulong), ulong.MinValue, ulong.MaxValue);
        PrintTypeInformation("float", sizeof(float), float.MinValue, float.MaxValue);
        PrintTypeInformation("double", sizeof(double), double.MinValue, double.MaxValue);
        PrintTypeInformation("decimal", sizeof(decimal), decimal.MinValue, decimal.MaxValue);

        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════════╝");

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    static void PrintTypeInformation(string typeName, int byteSize, object minValue, object maxValue)
    {
        // 左对齐格式化输出：类型名(12位)、字节数(10位)、最小值(30位)、最大值(30位)
        Console.WriteLine($"║ {typeName,-12} {byteSize,-15} {minValue,-30} {maxValue,-30} ║");
    }
}