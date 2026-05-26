using OrderManagement;

var tests = new (string Name, Action Run)[]
{
    ("AddOrder assigns ids, stores customers, and rejects empty orders", AddOrderTests),
    ("DeleteOrder removes existing orders and rejects missing ids", DeleteOrderTests),
    ("UpdateOrder replaces existing orders and validates details", UpdateOrderTests),
    ("GetAllOrders, SetSortKey, and ResetSort return expected order", SortTests),
    ("QueryByOrderId returns matching orders ordered by amount", QueryByOrderIdTests),
    ("QueryByProduct supports name, id, and numeric product queries", QueryByProductTests),
    ("QueryByCustomer supports name and id queries", QueryByCustomerTests),
    ("QueryByTotalAmount returns orders at or above the minimum", QueryByTotalAmountTests),
    ("FindCustomer returns known customers and null for missing ids", FindCustomerTests),
    ("Export and Import round-trip XML and preserve next generated id", ExportImportTests)
};

int passed = 0;
int failed = 0;

foreach (var test in tests)
{
    try
    {
        test.Run();
        passed++;
        Console.WriteLine($"[PASS] {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"[FAIL] {test.Name}");
        Console.WriteLine($"       {ex.GetType().Name}: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine($"Total: {tests.Length}, Passed: {passed}, Failed: {failed}");

if (failed > 0)
{
    Environment.Exit(1);
}

static void AddOrderTests()
{
    var service = new OrderService();
    var first = CreateOrder("C001", "张三", ("P001", "笔记本电脑", 5999m, 1));
    var second = CreateOrder("C001", "张三新名", ("P002", "机械键盘", 399m, 2));

    service.AddOrder(first);
    service.AddOrder(second);

    AssertEqual("1", first.OrderId, "first order id");
    AssertEqual("2", second.OrderId, "second order id");
    AssertTrue(ReferenceEquals(first.Customer, second.Customer), "same customer id should reuse the existing customer object");
    ExpectThrows<OrderException>(() => service.AddOrder(new Order(new Customer("C002", "李四"))), "empty orders should be rejected");
}

static void DeleteOrderTests()
{
    var service = SeedService();

    service.DeleteOrder("1");

    AssertEqual(1, service.GetAllOrders().Count, "remaining order count");
    ExpectThrows<OrderException>(() => service.DeleteOrder("404"), "missing order deletion should throw");
}

static void UpdateOrderTests()
{
    var service = SeedService();
    var replacement = CreateOrder("C003", "王五", ("P003", "无线鼠标", 199m, 3));
    replacement.OrderId = "1";

    service.UpdateOrder(replacement);

    var updated = service.QueryByOrderId("1").Single();
    AssertEqual("王五", updated.Customer.Name, "updated customer name");
    AssertEqual(597m, updated.TotalAmount, "updated total amount");

    var missing = CreateOrder("C004", "赵六", ("P001", "笔记本电脑", 5999m, 1));
    missing.OrderId = "404";
    ExpectThrows<OrderException>(() => service.UpdateOrder(missing), "updating a missing order should throw");

    var empty = new Order(new Customer("C005", "钱七"))
    {
        OrderId = "2"
    };
    ExpectThrows<OrderException>(() => service.UpdateOrder(empty), "empty replacement should throw");
}

static void SortTests()
{
    var service = SeedService();

    service.SetSortKey((Order o) => o.TotalAmount);
    var amountSorted = service.GetAllOrders();
    AssertEqual("2", amountSorted[0].OrderId, "lowest amount should come first");

    service.ResetSort();
    var idSorted = service.GetAllOrders();
    AssertEqual("1", idSorted[0].OrderId, "default sort should return order id 1 first");
}

static void QueryByOrderIdTests()
{
    var service = SeedService();

    var result = service.QueryByOrderId("1");

    AssertEqual(1, result.Count, "order id query count");
    AssertEqual("1", result[0].OrderId, "order id query result");
    ExpectThrows<OrderException>(() => service.QueryByOrderId("404"), "missing order id query should throw");
}

static void QueryByProductTests()
{
    var service = SeedService();

    AssertEqual("1", service.QueryByProduct("笔记本").Single().OrderId, "query by product name");
    AssertEqual("2", service.QueryByProduct("P002").Single().OrderId, "query by product id");
    AssertEqual("2", service.QueryByProduct("2").Single().OrderId, "query by product number");
    ExpectThrows<OrderException>(() => service.QueryByProduct("不存在的商品"), "missing product query should throw");
}

static void QueryByCustomerTests()
{
    var service = SeedService();

    AssertEqual("1", service.QueryByCustomer("张").Single().OrderId, "query by customer name");
    AssertEqual("2", service.QueryByCustomer("C002").Single().OrderId, "query by customer id");
    ExpectThrows<OrderException>(() => service.QueryByCustomer("不存在的客户"), "missing customer query should throw");
}

static void QueryByTotalAmountTests()
{
    var service = SeedService();

    var result = service.QueryByTotalAmount(500m);

    AssertEqual(1, result.Count, "minimum amount query count");
    AssertEqual("1", result[0].OrderId, "minimum amount query result");
    ExpectThrows<OrderException>(() => service.QueryByTotalAmount(100000m), "unmatched amount query should throw");
}

static void FindCustomerTests()
{
    var service = SeedService();

    AssertEqual("张三", service.FindCustomer("C001")?.Name, "found customer name");
    AssertTrue(service.FindCustomer("missing") == null, "missing customer should be null");
}

static void ExportImportTests()
{
    var source = SeedService();
    string filePath = Path.Combine(Path.GetTempPath(), $"orders-{Guid.NewGuid():N}.xml");

    try
    {
        source.Export(filePath);
        AssertTrue(File.Exists(filePath), "export should create an XML file");

        var imported = new OrderService();
        imported.Import(filePath);

        var orders = imported.GetAllOrders();
        AssertEqual(2, orders.Count, "imported order count");
        AssertEqual("张三", imported.FindCustomer("C001")?.Name, "imported customer");
        AssertEqual(5999m, imported.QueryByOrderId("1").Single().TotalAmount, "imported total amount");

        var next = CreateOrder("C003", "王五", ("P003", "无线鼠标", 199m, 1));
        imported.AddOrder(next);
        AssertEqual("3", next.OrderId, "next id after import");
    }
    finally
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}

static OrderService SeedService()
{
    var service = new OrderService();
    service.AddOrder(CreateOrder("C001", "张三", ("P001", "笔记本电脑", 5999m, 1)));
    service.AddOrder(CreateOrder("C002", "李四", ("P002", "机械键盘", 399m, 1)));
    return service;
}

static Order CreateOrder(string customerId, string customerName, params (string Id, string Name, decimal Price, int Quantity)[] details)
{
    var order = new Order(new Customer(customerId, customerName), new DateTime(2026, 5, 26, 9, 0, 0));

    foreach (var detail in details)
    {
        var product = new Product(detail.Id, detail.Name, detail.Price);
        order.AddDetail(new OrderDetails(product, detail.Quantity, detail.Price));
    }

    return order;
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message}: expected {expected}, got {actual}");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void ExpectThrows<TException>(Action action, string message) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"{message}: expected {typeof(TException).Name}, got {ex.GetType().Name}");
    }

    throw new InvalidOperationException($"{message}: expected {typeof(TException).Name}, but no exception was thrown");
}
