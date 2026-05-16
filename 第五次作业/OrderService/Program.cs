using OrderManagement.Models;
using OrderManagement.Services;

namespace OrderManagement
{
    class Program
    {
        static OrderService orderService = new();
        static List<Product> products = new()
        {
            new Product("P001", "笔记本电脑", 5999m),
            new Product("P002", "机械键盘", 399m),
            new Product("P003", "无线鼠标", 199m),
            new Product("P004", "显示器", 1499m),
            new Product("P005", "耳机", 299m)
        };

        static void Main(string[] args)
        {
            Console.WriteLine("===== 订单管理系统 =====");
            while (true)
            {
                Console.WriteLine("\n请选择操作：");
                Console.WriteLine("1. 添加订单");
                Console.WriteLine("2. 删除订单");
                Console.WriteLine("3. 修改订单");
                Console.WriteLine("4. 查询订单");
                Console.WriteLine("5. 显示所有订单");
                Console.WriteLine("6. 自定义排序");
                Console.WriteLine("7. 导出订单到 XML");
                Console.WriteLine("8. 从 XML 导入订单");
                Console.WriteLine("0. 退出");
                Console.Write("输入: ");
                string? input = Console.ReadLine();
                try
                {
                    switch (input)
                    {
                        case "1": AddOrder(); break;
                        case "2": DeleteOrder(); break;
                        case "3": UpdateOrder(); break;
                        case "4": QueryOrder(); break;
                        case "5": ShowAllOrders(); break;
                        case "6": CustomSort(); break;
                        case "7": ExportOrders(); break;
                        case "8": ImportOrders(); break;
                        case "0": return;
                        default: Console.WriteLine("无效选项，请重新输入。"); break;
                    }
                }
                catch (OrderException ex)
                {
                    Console.WriteLine($"错误: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"系统错误: {ex.Message}");
                }
            }
        }

        static void AddOrder()
        {
            Console.Write("输入客户ID: ");
            string customerId = Console.ReadLine()!;
            Customer customer;
            var existing = orderService.FindCustomer(customerId);
            if (existing != null)
            {
                Console.WriteLine($"已存在客户: {existing.Name}");
                customer = existing;
            }
            else
            {
                while (true)
                {
                    Console.Write("输入客户姓名: ");
                    string customerName = Console.ReadLine()!;
                    try
                    {
                        customer = new Customer(customerId, customerName);
                        break;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"错误：{ex.Message}");
                        Console.WriteLine("请重新输入。");
                    }
                }
            }

            Order order = new(customer);
            Console.WriteLine("\n添加订单明细（按 Enter 结束）：");
            while (true)
            {
                Console.WriteLine("可选货物：");
                products.ForEach(p => Console.WriteLine($"  {p.ProductId} : {p.Name} - {p.Price:C}"));
                Console.Write("输入货物编号: ");
                string input = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(input)) break;

                try
                {
                    int productNumber = Product.ParseProductNumber(input);
                    Product? product = products.FirstOrDefault(p => p.ProductNumber == productNumber);
                    if (product == null)
                    {
                        Console.WriteLine("无效货物编号，重新输入。");
                        continue;
                    }
                    Console.Write("数量: ");
                    if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                    {
                        Console.WriteLine("数量不合法。");
                        continue;
                    }
                    OrderDetails detail = new(product, qty, product.Price);
                    order.AddDetail(detail);
                    Console.WriteLine("明细添加成功。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"输入错误: {ex.Message}");
                }
            }

            orderService.AddOrder(order);
            Console.WriteLine($"订单创建成功！订单号为: {order.OrderId}");
        }

        static void DeleteOrder()
        {
            Console.Write("输入要删除的订单号: ");
            string orderId = Console.ReadLine()!;
            orderService.DeleteOrder(orderId);
            Console.WriteLine("订单删除成功。");
        }

        static void UpdateOrder()
        {
            Console.Write("输入要修改的订单号: ");
            string orderId = Console.ReadLine()!;

            Console.Write("输入新客户ID: ");
            string customerId = Console.ReadLine()!;
            Customer customer;
            var existingCust = orderService.FindCustomer(customerId);
            if (existingCust != null)
            {
                Console.WriteLine($"已存在客户: {existingCust.Name}");
                customer = existingCust;
            }
            else
            {
                while (true)
                {
                    Console.Write("输入新客户姓名: ");
                    string customerName = Console.ReadLine()!;
                    try
                    {
                        customer = new Customer(customerId, customerName);
                        break;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"错误：{ex.Message}");
                        Console.WriteLine("请重新输入。");
                    }
                }
            }

            Order updatedOrder = new(customer)
            {
                OrderId = orderId
            };

            Console.WriteLine("重新输入订单明细（按 Enter 结束）:");
            while (true)
            {
                Console.WriteLine("可选货物：");
                products.ForEach(p => Console.WriteLine($"  {p.ProductId} ({p.ProductNumber}) : {p.Name} - {p.Price:C}"));
                Console.Write("输入货物编号或数字: ");
                string input = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(input)) break;
                try
                {
                    int productNumber = Product.ParseProductNumber(input);
                    Product? product = products.FirstOrDefault(p => p.ProductNumber == productNumber);
                    if (product == null)
                    {
                        Console.WriteLine("无效货物编号。");
                        continue;
                    }
                    Console.Write("数量: ");
                    if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                    {
                        Console.WriteLine("数量不合法。");
                        continue;
                    }
                    OrderDetails detail = new(product, qty, product.Price);
                    updatedOrder.AddDetail(detail);
                    Console.WriteLine("明细添加成功。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"输入错误: {ex.Message}");
                }
            }

            orderService.UpdateOrder(updatedOrder);
            Console.WriteLine("订单修改成功。");
        }

        static void QueryOrder()
        {
            Console.WriteLine("\n查询方式：");
            Console.WriteLine("1. 按订单号");
            Console.WriteLine("2. 按商品");
            Console.WriteLine("3. 按客户");
            Console.WriteLine("4. 按订单金额（大于等于）");
            Console.Write("选择: ");
            string? choice = Console.ReadLine();
            List<Order> result;
            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Write("输入订单号关键字: ");
                        result = orderService.QueryByOrderId(Console.ReadLine()!);
                        break;
                    case "2":
                        Console.Write("输入商品名称关键字: ");
                        result = orderService.QueryByProduct(Console.ReadLine()!);
                        break;
                    case "3":
                        Console.Write("输入客户姓名关键字: ");
                        result = orderService.QueryByCustomer(Console.ReadLine()!);
                        break;
                    case "4":
                        Console.Write("输入最低金额: ");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                            throw new OrderException("金额格式错误。");
                        result = orderService.QueryByTotalAmount(amount);
                        break;
                    default:
                        Console.WriteLine("无效选项。");
                        return;
                }
                Console.WriteLine($"\n查询结果（共 {result.Count} 条，按总金额升序）：");
                foreach (var order in result)
                {
                    Console.WriteLine("----------------------------------------");
                    Console.WriteLine(order);
                }
            }
            catch (OrderException ex)
            {
                Console.WriteLine($"查询失败: {ex.Message}");
            }
        }

        static void ShowAllOrders()
        {
            var orders = orderService.GetAllOrders();
            if (orders.Count == 0)
            {
                Console.WriteLine("当前没有订单。");
                return;
            }
            Console.WriteLine("\n所有订单：");
            foreach (var order in orders)
            {
                Console.WriteLine("========================================");
                Console.WriteLine(order);
            }
        }

        static void CustomSort()
        {
            Console.WriteLine("\n自定义排序选项：");
            Console.WriteLine("1. 默认排序（按订单号）");
            Console.WriteLine("2. 按总金额升序");
            Console.WriteLine("3. 按总金额降序");
            Console.WriteLine("4. 按订单时间升序");
            Console.WriteLine("5. 按订单时间降序");
            Console.Write("选择: ");
            string? choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        orderService.ResetSort();
                        break;
                    case "2":
                        orderService.SetSortKey((Order o) => o.TotalAmount);
                        break;
                    case "3":
                        orderService.SetSortKey((Order o) => -o.TotalAmount);
                        break;
                    case "4":
                        orderService.SetSortKey((Order o) => o.OrderDate.Ticks);
                        break;
                    case "5":
                        orderService.SetSortKey((Order o) => -o.OrderDate.Ticks);
                        break;
                    default:
                        Console.WriteLine("无效选项，恢复默认排序。");
                        orderService.ResetSort();
                        break;
                }
                Console.WriteLine("排序规则已更新。使用“显示所有订单”查看效果。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"排序设置失败: {ex.Message}");
            }
        }

        static void ExportOrders()
        {
            Console.Write("请输入要保存的 XML 文件路径: ");
            string path = Console.ReadLine()!;
            try
            {
                orderService.Export(path);
                Console.WriteLine($"订单已成功导出到 {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出失败: {ex.Message}");
            }
        }

        static void ImportOrders()
        {
            Console.Write("请输入要导入的 XML 文件路径: ");
            string path = Console.ReadLine()!;
            try
            {
                orderService.Import(path);
                Console.WriteLine($"订单已从 {path} 导入成功，当前共有 {orderService.GetAllOrders().Count} 条订单。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导入失败: {ex.Message}");
            }
        }
    }
}