using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("请输入两个非负整数，将为您输出上下限之间的所有素数（包含两端）");

        uint lowerLimit = GetNonNegativeNum("请输入下限：");

        uint higherLimit;
        do
        {
            higherLimit = GetNonNegativeNum("请输入上限：");

            if (lowerLimit > higherLimit)
            {
                Console.WriteLine($"下限{lowerLimit}大于上限{higherLimit}，请重新输入一个更大的上限！");
            }
        } while (lowerLimit > higherLimit);

        Console.WriteLine($"将为您找到下限{lowerLimit}与上限{higherLimit}之间的所有素数");
        List<uint> primeList = new List<uint>();
        FindPrimeinRange(lowerLimit, higherLimit, primeList);

        uint[] primeArray = primeList.ToArray();
        Console.WriteLine($"{lowerLimit}到{higherLimit}之间的素数为：");
        if (primeArray.Length == 0)
        {
            Console.WriteLine("无");
        }
        else
        {
            Console.WriteLine(string.Join(", ", primeArray));
        }

        return;
    }

    static uint GetNonNegativeNum(string s)
    { // 获取非负整数
        uint num;
        while (true) {
            Console.Write(s);

            string s_num = Console.ReadLine();
            if (uint.TryParse(s_num, out num))
            {
                break;
            }
            else
            {
                Console.WriteLine("请输入一个非负整数！");
            }
        }

        return num;
    }

    static void FindPrimeinRange(uint lowerLimit, uint higherLimit, List<uint> primeList)
    { // 获取范围内的素数
        // 素数缓冲区
        List<uint> primeBuffer = new List<uint>();

        uint lowerSqrt = (uint)Math.Floor(Math.Sqrt(lowerLimit));
        uint higherSqrt = (uint)Math.Floor(Math.Sqrt(higherLimit));

        // 先处理最小的素数2
        if (higherLimit >= 2)
        {
            primeBuffer.Add(2);
            if (2 >= lowerLimit)
            {
                primeList.Add(2);
            }
        }

        // 获取小于上限平方根的所有素数
        // 将其存储到primeBuffer中
        uint num = 3;
        for (; num <= higherSqrt; num += 2)
        {
            bool isPrime = true;
            uint numSqrt = (uint)Math.Sqrt(num);

            foreach (uint p in primeBuffer)
            {
                if (p > numSqrt) break;

                if (num % p == 0)
                {
                    isPrime = false;
                    break;
                }
            }

            if(isPrime)
            {
                primeBuffer.Add(num);

                if (num >= lowerLimit)
                { // 当上限的平方根大于下限本身时
                    primeList.Add(num);
                }
            }
        }

        if (primeList.Any())
        { // 若已经向区间内素数序列添加了素数，则从最大的开始
            num = primeList.Last() + 2;
        }
        else
        { // 否则从lowerlimit开始
            num = lowerLimit % 2 == 0 ? lowerLimit + 1 : lowerLimit;
        }

        for (; num <= higherLimit; num += 2)
        { // 寻找区间内其余的素数
            bool isPrime = true;
            uint numSqrt = (uint)Math.Sqrt(num);

            foreach (uint p in primeBuffer)
            {
                if (p > numSqrt) break;

                if (num % p == 0)
                {  
                    isPrime = false;
                    break;
                }
            }

            if (isPrime) primeList.Add(num);
        }
    }
}