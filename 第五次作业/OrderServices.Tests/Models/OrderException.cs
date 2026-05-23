using System;

namespace OrderManagement.Models
{
    public class OrderException : ApplicationException
    {
        public OrderException(string message) : base(message) { }
    }
}