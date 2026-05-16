using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderManagement
{
    // 自定义异常
    public class OrderException : ApplicationException
    {
        public OrderException(string message) : base(message) { }
    }

    // 货物类
    public class Product
    {
        public string ProductId { get; init; }      // 如 "P001"
        public int ProductNumber { get; init; }     // 从 ProductId 提取的数字，如 1
        public string Name { get; init; }
        public decimal Price { get; init; }

        public Product(string productId, string name, decimal price)
        {
            ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
            Name = name;
            Price = price;
            // 从 "P001" 提取 1
            if (productId.Length > 1 && productId[0] == 'P')
                ProductNumber = int.Parse(productId[1..]);
            else
                throw new ArgumentException("货物编号格式必须为 PXXX");
        }

        // 解析用户输入
        public static int ParseProductNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("输入不能为空");
            input = input.Trim().ToUpper();
            if (input.StartsWith('P'))
                return int.Parse(input[1..]);
            else
                return int.Parse(input);
        }

        public override bool Equals(object? obj)
        {
            return obj is Product product &&
                   ProductId == product.ProductId;
        }

        public override int GetHashCode() => ProductId.GetHashCode();

        public override string ToString() => $"{ProductId}: {Name}, 单价: {Price:C}";
    }

    // 客户类
    public class Customer
    {
        public string CustomerId { get; init; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("客户姓名不能为空");
                if (value.All(char.IsDigit))
                    throw new ArgumentException("客户姓名不能为纯数字，请使用文字姓名。");
                _name = value;
            }
        }

        public Customer(string customerId, string name)
        {
            _name = string.Empty;
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
            Name = name; // 通过属性 setter 触发验证
        }

        public override bool Equals(object? obj)
        {
            return obj is Customer customer &&
                   CustomerId == customer.CustomerId;
        }

        public override int GetHashCode() => CustomerId.GetHashCode();

        public override string ToString() => $"客户[{CustomerId}]: {Name}";
    }

    // 订单明细类
    public class OrderDetails
    {
        public Product Product { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal TotalPrice => Quantity * UnitPrice;

        public OrderDetails(Product product, int quantity, decimal unitPrice)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            if (quantity <= 0) throw new ArgumentException("数量必须大于0");
            if (unitPrice <= 0) throw new ArgumentException("单价必须大于0");
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderDetails details &&
                   EqualityComparer<Product>.Default.Equals(Product, details.Product) &&
                   Quantity == details.Quantity &&
                   UnitPrice == details.UnitPrice;
        }

        public override int GetHashCode() => HashCode.Combine(Product, Quantity, UnitPrice);

        public override string ToString() =>
            $"\t{Product.Name} ×{Quantity} @ {UnitPrice:C} = {TotalPrice:C}";
    }

    // 订单类
    public class Order
    {
        public string OrderId { get; internal set; }   // 由 OrderService 自动生成
        public Customer Customer { get; set; }
        public List<OrderDetails> Details { get; private set; } = new();
        public DateTime OrderDate { get; init; }
        public decimal TotalAmount => Details.Sum(d => d.TotalPrice);

        // 内部创建时使用（OrderService 会随后设置 OrderId）
        public Order(Customer customer, DateTime? orderDate = null)
        {
            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            OrderDate = orderDate ?? DateTime.Now;
            OrderId = string.Empty; // 待服务分配
        }

        public void AddDetail(OrderDetails detail)
        {
            if (Details.Contains(detail))
                throw new OrderException("该订单明细已存在，不可重复添加。");
            Details.Add(detail);
        }

        public void RemoveDetail(OrderDetails detail)
        {
            if (!Details.Remove(detail))
                throw new OrderException("未找到指定的订单明细。");
        }

        public override bool Equals(object? obj)
        {
            return obj is Order order &&
                   OrderId == order.OrderId;
        }

        public override int GetHashCode() => OrderId.GetHashCode();

        public override string ToString()
        {
            string detailsStr = string.Join(Environment.NewLine, Details.Select(d => d.ToString()));
            return $"订单号: {OrderId}\n客户: {Customer}\n日期: {OrderDate:yyyy-MM-dd HH:mm:ss}\n明细:\n{detailsStr}\n总金额: {TotalAmount:C}";
        }
    }

    // 订单服务类
    public class OrderService
    {
        // 主数据存储（数据库）
        private List<Order> orders = new();
        // 客户字典
        private Dictionary<string, Customer> customers = new();
        // 订单号自动生成器
        private int nextOrderId = 1;

        // 自定义排序键，null 表示默认按订单号排序
        private Func<Order, IComparable>? sortKeySelector = null;

        // 添加订单
        public void AddOrder(Order order)
        {
            if (order.Details.Count == 0)
                throw new OrderException("订单必须包含至少一个明细。");

            // 自动分配订单号
            order.OrderId = (nextOrderId++).ToString();

            // 客户处理：如果客户ID已存在，则使用已有客户；否则添加到客户字典
            if (customers.TryGetValue(order.Customer.CustomerId, out var existingCustomer))
            {
                order.Customer = existingCustomer;
            }
            else
            {
                customers.Add(order.Customer.CustomerId, order.Customer);
            }

            orders.Add(order);
        }

        // 删除订单
        public void DeleteOrder(string orderId)
        {
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                throw new OrderException($"未找到订单号为 {orderId} 的订单，删除失败。");
            orders.Remove(order);
        }

        // 修改订单（覆盖原有信息）
        public void UpdateOrder(Order updatedOrder)
        {
            var existing = orders.FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);
            if (existing == null)
                throw new OrderException($"未找到订单号为 {updatedOrder.OrderId} 的订单，修改失败。");
            if (updatedOrder.Details.Count == 0)
                throw new OrderException("订单必须包含至少一个明细。");

            // 处理客户更新（与添加逻辑相同）
            if (customers.TryGetValue(updatedOrder.Customer.CustomerId, out var existingCustomer))
            {
                updatedOrder.Customer = existingCustomer;
            }
            else
            {
                customers.Add(updatedOrder.Customer.CustomerId, updatedOrder.Customer);
            }

            // 替换原订单
            int index = orders.IndexOf(existing);
            orders[index] = updatedOrder;
        }

        // 恢复默认排序（按订单号）
        public void ResetSort() => sortKeySelector = null;

        // 设置自定义排序键（Lambda 表达式）
        public void SetSortKey<TKey>(Func<Order, TKey> keySelector) where TKey : IComparable
        {
            sortKeySelector = o => keySelector(o);
        }

        // 获取所有订单（按当前排序规则排序后的新列表）
        public List<Order> GetAllOrders()
        {
            IEnumerable<Order> result = orders;
            if (sortKeySelector == null)
                result = new List<Order>(orders);
            else
                result = result.OrderBy(sortKeySelector);
            return result.ToList();
        }

        // 按订单号查询（结果按总金额升序）
        public List<Order> QueryByOrderId(string orderId)
        {
            var query = orders.Where(o => o.OrderId.Contains(orderId, StringComparison.OrdinalIgnoreCase))
                             .OrderBy(o => o.TotalAmount);
            var result = query.ToList();
            if (result.Count == 0)
                throw new OrderException("未找到匹配的订单。");
            return result;
        }

        // 按商品名称查询
        public List<Order> QueryByProduct(string keyword)
        {
            // 尝试解析为数字
            int? productNumber = null;
            if (int.TryParse(keyword, out int num))
                productNumber = num;

            var query = orders.Where(o => o.Details.Any(d =>
                d.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                d.Product.ProductId.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (productNumber.HasValue && d.Product.ProductNumber == productNumber.Value)))
                .OrderBy(o => o.TotalAmount);

            var result = query.ToList();
            if (result.Count == 0)
                throw new OrderException("未找到包含该商品的订单。");
            return result;
        }

        // 按客户姓名查询
        public List<Order> QueryByCustomer(string keyword)
        {
            var query = orders.Where(o => o.Customer.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) || 
                                    o.Customer.CustomerId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(o => o.TotalAmount);
            var result = query.ToList();
            if (result.Count == 0)
                throw new OrderException("未找到该客户的订单。");
            return result;
        }

        // 按订单金额查询（大于等于）
        public List<Order> QueryByTotalAmount(decimal minAmount)
        {
            var query = orders.Where(o => o.TotalAmount >= minAmount)
                             .OrderBy(o => o.TotalAmount);
            var result = query.ToList();
            if (result.Count == 0)
                throw new OrderException("未找到金额大于等于指定值的订单。");
            return result;
        }

        // 根据客户ID查找客户（供外部使用）
        public Customer? FindCustomer(string customerId)
        {
            customers.TryGetValue(customerId, out var customer);
            return customer;
        }
    }

    class Program
    {
        static OrderService orderService = new();
        // 预定义货物列表
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
            // 处理客户
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
            // 此时订单号尚未分配，先添加明细，提交时自动生成并显示
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

            // 重新输入客户及明细
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
                OrderId = orderId // 保留原订单号
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
                        orderService.SetSortKey((Order o) => o.TotalAmount); // 升序
                        break;
                    case "3":
                        orderService.SetSortKey((Order o) => -o.TotalAmount); // 降序
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
    }
}