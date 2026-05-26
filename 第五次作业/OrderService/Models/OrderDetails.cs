namespace OrderManagement;

public class OrderDetails
{
    public Product Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    public OrderDetails()
    {
    }

    public OrderDetails(Product product, int quantity, decimal unitPrice)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        if (quantity <= 0)
        {
            throw new ArgumentException("数量必须大于0");
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentException("单价必须大于0");
        }

        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public override bool Equals(object? obj)
    {
        return obj is OrderDetails details
            && EqualityComparer<Product>.Default.Equals(Product, details.Product)
            && Quantity == details.Quantity
            && UnitPrice == details.UnitPrice;
    }

    public override int GetHashCode() => HashCode.Combine(Product, Quantity, UnitPrice);

    public override string ToString() =>
        $"\t{Product.Name} ×{Quantity} @ {UnitPrice:C} = {TotalPrice:C}";
}
