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

        // ===================== 输出所有数值类型信息 =====================
        // sbyte：有符号字节型
        PrintTypeInfo("sbyte", sizeof(sbyte), sbyte.MinValue, sbyte.MaxValue);
        // byte：无符号字节型
        PrintTypeInfo("byte", sizeof(byte), byte.MinValue, byte.MaxValue);
        // short：有符号短整型
        PrintTypeInfo("short", sizeof(short), short.MinValue, short.MaxValue);
        // ushort：无符号短整型
        PrintTypeInfo("ushort", sizeof(ushort), ushort.MinValue, ushort.MaxValue);
        // int：有符号整型
        PrintTypeInfo("int", sizeof(int), int.MinValue, int.MaxValue);
        // uint：无符号整型
        PrintTypeInfo("uint", sizeof(uint), uint.MinValue, uint.MaxValue);
        // long：有符号长整型
        PrintTypeInfo("long", sizeof(long), long.MinValue, long.MaxValue);
        // ulong：无符号长整型
        PrintTypeInfo("ulong", sizeof(ulong), ulong.MinValue, ulong.MaxValue);
        // float：单精度浮点型
        PrintTypeInfo("float", sizeof(float), float.MinValue, float.MaxValue);
        // double：双精度浮点型
        PrintTypeInfo("double", sizeof(double), double.MinValue, double.MaxValue);
        // decimal：高精度十进制型
        PrintTypeInfo("decimal", sizeof(decimal), decimal.MinValue, decimal.MaxValue);

        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════════╝");
    }

    /// <summary>
    /// 封装输出方法：统一格式打印类型信息（保证文本对齐）
    /// </summary>
    static void PrintTypeInfo(string typeName, int byteSize, object minValue, object maxValue)
    {
        // 左对齐格式化输出：类型名(12位)、字节数(10位)、最小值(30位)、最大值(30位)
        Console.WriteLine($"║ {typeName,-12} {byteSize,-15} {minValue,-30} {maxValue,-30} ║");
    }
}