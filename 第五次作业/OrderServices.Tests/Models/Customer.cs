using System;
using System.Linq;

namespace OrderManagement.Models
{
    public class Customer
    {
        public string CustomerId { get; set; }
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

        public Customer() { } // 无参构造函数

        public Customer(string customerId, string name)
        {
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            return obj is Customer customer &&
                   CustomerId == customer.CustomerId;
        }

        public override int GetHashCode() => CustomerId.GetHashCode();

        public override string ToString() => $"客户[{CustomerId}]: {Name}";
    }
}