using System;
using System.Xml.Serialization;

namespace OrderManagement.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        [XmlIgnore]
        public int ProductNumber
        {
            get
            {
                if (ProductId?.Length > 1 && ProductId[0] == 'P')
                    return int.Parse(ProductId[1..]);
                throw new InvalidOperationException("货物编号格式必须为 PXXX");
            }
        }

        // 无参构造函数，供 XML 序列化使用
        public Product() { }

        public Product(string productId, string name, decimal price)
        {
            ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
            Name = name;
            Price = price;
        }

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
}