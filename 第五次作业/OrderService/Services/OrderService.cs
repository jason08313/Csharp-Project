using System.Xml.Serialization;

namespace OrderManagement;

public class OrderService
{
    private List<Order> orders = new();
    private Dictionary<string, Customer> customers = new();
    private int nextOrderId = 1;
    private Func<Order, IComparable>? sortKeySelector;

    public void AddOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.Details.Count == 0)
        {
            throw new OrderException("订单必须包含至少一个明细。");
        }

        order.OrderId = (nextOrderId++).ToString();
        NormalizeCustomer(order);
        orders.Add(order);
    }

    public void DeleteOrder(string orderId)
    {
        var order = orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new OrderException($"未找到订单号为 {orderId} 的订单，删除失败。");
        }

        orders.Remove(order);
    }

    public void UpdateOrder(Order updatedOrder)
    {
        ArgumentNullException.ThrowIfNull(updatedOrder);

        var existing = orders.FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);
        if (existing == null)
        {
            throw new OrderException($"未找到订单号为 {updatedOrder.OrderId} 的订单，修改失败。");
        }

        if (updatedOrder.Details.Count == 0)
        {
            throw new OrderException("订单必须包含至少一个明细。");
        }

        NormalizeCustomer(updatedOrder);

        int index = orders.IndexOf(existing);
        orders[index] = updatedOrder;
    }

    public void ResetSort() => sortKeySelector = null;

    public void SetSortKey<TKey>(Func<Order, TKey> keySelector) where TKey : IComparable
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        sortKeySelector = o => keySelector(o);
    }

    public List<Order> GetAllOrders()
    {
        IEnumerable<Order> result = orders;

        if (sortKeySelector == null)
        {
            result = result.OrderBy(o => TryParseOrderId(o.OrderId, out int id) ? id : int.MaxValue)
                .ThenBy(o => o.OrderId);
        }
        else
        {
            result = result.OrderBy(sortKeySelector);
        }

        return result.ToList();
    }

    public List<Order> QueryByOrderId(string orderId)
    {
        var result = orders.Where(o => o.OrderId.Contains(orderId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(o => o.TotalAmount)
            .ToList();

        if (result.Count == 0)
        {
            throw new OrderException("未找到匹配的订单。");
        }

        return result;
    }

    public List<Order> QueryByProduct(string keyword)
    {
        int? productNumber = null;
        if (int.TryParse(keyword, out int number))
        {
            productNumber = number;
        }

        var result = orders.Where(o => o.Details.Any(d =>
                d.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || d.Product.ProductId.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || (productNumber.HasValue && d.Product.ProductNumber == productNumber.Value)))
            .OrderBy(o => o.TotalAmount)
            .ToList();

        if (result.Count == 0)
        {
            throw new OrderException("未找到包含该商品的订单。");
        }

        return result;
    }

    public List<Order> QueryByCustomer(string keyword)
    {
        var result = orders.Where(o =>
                o.Customer.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || o.Customer.CustomerId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderBy(o => o.TotalAmount)
            .ToList();

        if (result.Count == 0)
        {
            throw new OrderException("未找到该客户的订单。");
        }

        return result;
    }

    public List<Order> QueryByTotalAmount(decimal minAmount)
    {
        var result = orders.Where(o => o.TotalAmount >= minAmount)
            .OrderBy(o => o.TotalAmount)
            .ToList();

        if (result.Count == 0)
        {
            throw new OrderException("未找到金额大于等于指定值的订单。");
        }

        return result;
    }

    public Customer? FindCustomer(string customerId)
    {
        customers.TryGetValue(customerId, out var customer);
        return customer;
    }

    public void Export(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var serializer = new XmlSerializer(typeof(List<Order>));
        using var stream = File.Create(filePath);
        serializer.Serialize(stream, orders);
    }

    public void Import(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("未找到要导入的XML文件。", filePath);
        }

        var serializer = new XmlSerializer(typeof(List<Order>));
        using var stream = File.OpenRead(filePath);
        var importedOrders = serializer.Deserialize(stream) as List<Order>;

        if (importedOrders == null)
        {
            throw new OrderException("XML文件中没有有效的订单数据。");
        }

        ValidateImportedOrders(importedOrders);

        orders = importedOrders;
        customers = new Dictionary<string, Customer>();
        foreach (var order in orders)
        {
            NormalizeCustomer(order);
        }

        nextOrderId = orders.Select(o => TryParseOrderId(o.OrderId, out int id) ? id : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
        ResetSort();
    }

    private void NormalizeCustomer(Order order)
    {
        if (customers.TryGetValue(order.Customer.CustomerId, out var existingCustomer))
        {
            order.Customer = existingCustomer;
        }
        else
        {
            customers.Add(order.Customer.CustomerId, order.Customer);
        }
    }

    private static void ValidateImportedOrders(List<Order> importedOrders)
    {
        var orderIds = new HashSet<string>();

        foreach (var order in importedOrders)
        {
            if (string.IsNullOrWhiteSpace(order.OrderId))
            {
                throw new OrderException("导入的订单缺少订单号。");
            }

            if (!orderIds.Add(order.OrderId))
            {
                throw new OrderException($"导入的订单号 {order.OrderId} 重复。");
            }

            if (string.IsNullOrWhiteSpace(order.Customer.CustomerId))
            {
                throw new OrderException($"订单 {order.OrderId} 缺少客户编号。");
            }

            if (order.Details.Count == 0)
            {
                throw new OrderException($"订单 {order.OrderId} 必须包含至少一个明细。");
            }

            foreach (var detail in order.Details)
            {
                if (string.IsNullOrWhiteSpace(detail.Product.ProductId))
                {
                    throw new OrderException($"订单 {order.OrderId} 中存在缺少编号的商品。");
                }

                if (detail.Quantity <= 0)
                {
                    throw new OrderException($"订单 {order.OrderId} 中存在数量不合法的明细。");
                }

                if (detail.UnitPrice <= 0)
                {
                    throw new OrderException($"订单 {order.OrderId} 中存在单价不合法的明细。");
                }
            }
        }
    }

    private static bool TryParseOrderId(string orderId, out int id)
    {
        return int.TryParse(orderId, out id);
    }
}
