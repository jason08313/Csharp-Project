using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using OrderManagement.Models;

namespace OrderManagement.Services
{
    public class OrderService
    {
        private List<Order> orders = new();
        private Dictionary<string, Customer> customers = new();
        private int nextOrderId = 1;
        private Func<Order, IComparable>? sortKeySelector = null;

        public void AddOrder(Order order)
        {
            if (order.Details.Count == 0)
                throw new OrderException("订单必须包含至少一个明细。");

            order.OrderId = (nextOrderId++).ToString();

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

        public void DeleteOrder(string orderId)
        {
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                throw new OrderException($"未找到订单号为 {orderId} 的订单，删除失败。");
            orders.Remove(order);
        }

        public void UpdateOrder(Order updatedOrder)
        {
            var existing = orders.FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);
            if (existing == null)
                throw new OrderException($"未找到订单号为 {updatedOrder.OrderId} 的订单，修改失败。");
            if (updatedOrder.Details.Count == 0)
                throw new OrderException("订单必须包含至少一个明细。");

            if (customers.TryGetValue(updatedOrder.Customer.CustomerId, out var existingCustomer))
            {
                updatedOrder.Customer = existingCustomer;
            }
            else
            {
                customers.Add(updatedOrder.Customer.CustomerId, updatedOrder.Customer);
            }

            int index = orders.IndexOf(existing);
            orders[index] = updatedOrder;
        }

        public void ResetSort() => sortKeySelector = null;

        public void SetSortKey<TKey>(Func<Order, TKey> keySelector) where TKey : IComparable
        {
            sortKeySelector = o => keySelector(o);
        }

        public List<Order> GetAllOrders()
        {
            IEnumerable<Order> result = orders;
            if (sortKeySelector == null)
                result = new List<Order>(orders);
            else
                result = result.OrderBy(sortKeySelector);
            return result.ToList();
        }

        public List<Order> QueryByOrderId(string orderId)
        {
            var query = orders.Where(o => o.OrderId.Contains(orderId, StringComparison.OrdinalIgnoreCase))
                             .OrderBy(o => o.TotalAmount);
            var result = query.ToList();
            if (result.Count == 0)
                throw new OrderException("未找到匹配的订单。");
            return result;
        }

        public List<Order> QueryByProduct(string keyword)
        {
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

        public List<Order> QueryByTotalAmount(decimal minAmount)
        {
            var query = orders.Where(o => o.TotalAmount >= minAmount)
                             .OrderBy(o => o.TotalAmount);
            var result = query.ToList();
            if (result.Count == 0)
                throw new OrderException("未找到金额大于等于指定值的订单。");
            return result;
        }

        public Customer? FindCustomer(string customerId)
        {
            customers.TryGetValue(customerId, out var customer);
            return customer;
        }

        // ========== 新增：XML 导出/导入 ==========
        public void Export(string filePath)
        {
            var settings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(filePath, settings);
            var serializer = new XmlSerializer(typeof(List<Order>));
            serializer.Serialize(writer, orders);
        }

        public void Import(string filePath)
        {
            if (!File.Exists(filePath))
                throw new OrderException($"文件不存在: {filePath}");

            var serializer = new XmlSerializer(typeof(List<Order>));
            using var reader = XmlReader.Create(filePath);
            var importedOrders = (List<Order>)serializer.Deserialize(reader)!;

            orders.Clear();
            customers.Clear();

            foreach (var order in importedOrders)
            {
                if (customers.TryGetValue(order.Customer.CustomerId, out var existingCust))
                    order.Customer = existingCust;
                else
                    customers.Add(order.Customer.CustomerId, order.Customer);

                orders.Add(order);
            }

            if (orders.Any())
                nextOrderId = orders.Max(o => int.Parse(o.OrderId)) + 1;
            else
                nextOrderId = 1;
        }
    }
}