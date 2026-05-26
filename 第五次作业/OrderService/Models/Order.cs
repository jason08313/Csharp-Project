using System.Xml.Serialization;

namespace OrderManagement;

public class Order
{
    public string OrderId { get; set; } = string.Empty;
    public Customer Customer { get; set; } = new();
    public List<OrderDetails> Details { get; set; } = new();
    public DateTime OrderDate { get; set; } = DateTime.Now;

    [XmlIgnore]
    public decimal TotalAmount => Details.Sum(d => d.TotalPrice);

    public Order()
    {
    }

    public Order(Customer customer, DateTime? orderDate = null)
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        OrderDate = orderDate ?? DateTime.Now;
    }

    public void AddDetail(OrderDetails detail)
    {
        if (Details.Contains(detail))
        {
            throw new OrderException("该订单明细已存在，不可重复添加。");
        }

        Details.Add(detail);
    }

    public void RemoveDetail(OrderDetails detail)
    {
        if (!Details.Remove(detail))
        {
            throw new OrderException("未找到指定的订单明细。");
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Order order && OrderId == order.OrderId;
    }

    public override int GetHashCode() => OrderId.GetHashCode();

    public override string ToString()
    {
        string detailsStr = string.Join(Environment.NewLine, Details.Select(d => d.ToString()));
        return $"订单号: {OrderId}\n客户: {Customer}\n日期: {OrderDate:yyyy-MM-dd HH:mm:ss}\n明细:\n{detailsStr}\n总金额: {TotalAmount:C}";
    }
}
