namespace OrderManagement;

public class Product
{
    public string ProductId { get; set; } = string.Empty;
    public int ProductNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Product()
    {
    }

    public Product(string productId, string name, decimal price)
    {
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        Name = name;
        Price = price;

        if (productId.Length > 1 && productId[0] == 'P')
        {
            ProductNumber = int.Parse(productId[1..]);
        }
        else
        {
            throw new ArgumentException("货物编号格式必须为 PXXX");
        }
    }

    public static int ParseProductNumber(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("输入不能为空");
        }

        input = input.Trim().ToUpper();
        return input.StartsWith('P') ? int.Parse(input[1..]) : int.Parse(input);
    }

    public override bool Equals(object? obj)
    {
        return obj is Product product && ProductId == product.ProductId;
    }

    public override int GetHashCode() => ProductId.GetHashCode();

    public override string ToString() => $"{ProductId}: {Name}, 单价: {Price:C}";
}
