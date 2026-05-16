using System;
using System.Reflection;

// 示例Person类，包含默认构造和带参构造
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    // 默认构造函数
    public Person()
    {
        Name = "未知";
        Age = 0;
    }

    // 带参数的构造函数
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public void Introduce()
    {
        Console.WriteLine($"姓名：{Name}，年龄：{Age}");
    }
}

class Program
{
    static void Main()
    {
        // 获取Person类的Type对象
        Type personType = typeof(Person);

        // 获取 Introduce 方法信息并校验
        MethodInfo? introduceMethod = personType.GetMethod("Introduce");
        if (introduceMethod == null)
        {
            Console.WriteLine("未找到 Introduce 方法，程序退出。\n");
            return;
        }

        // 一：默认构造函数
        Console.WriteLine("== 方式1: Activator.CreateInstance(默认构造函数) ==");
        try
        {
            object person1 = Activator.CreateInstance(personType);
            introduceMethod.Invoke(person1, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"反射创建默认对象失败：{ex.Message}");
        }

        // 二：带参构造函数
        Console.WriteLine("\n== 方式2: ConstructorInfo.Invoke(带参构造函数) ==");
        // 获取带参构造函数
        ConstructorInfo? ctor = personType.GetConstructor(new Type[] { typeof(string), typeof(int) });
        if (ctor != null)
        {
            try
            {
                object person2 = ctor.Invoke(new object[] { "张三", 28 }) ?? throw new InvalidOperationException("创建 Person 实例失败。");
                introduceMethod.Invoke(person2, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调用带参构造函数失败：{ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("未找到 Person(string, int) 构造函数。\n");
        }

        // 三：直接传参
        Console.WriteLine("\n== 方式3: Activator.CreateInstance 带参数 ==");
        try
        {
            object person3 = Activator.CreateInstance(personType, "李四", 30) ?? throw new InvalidOperationException("创建 Person 实例失败。");
            introduceMethod.Invoke(person3, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"带参数 Activator.CreateInstance 失败：{ex.Message}");
        }

        Console.WriteLine("按任意键继续...");
        Console.ReadKey(intercept: true);
    }
}