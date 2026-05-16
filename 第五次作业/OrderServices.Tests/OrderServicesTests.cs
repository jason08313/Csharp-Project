using System;
using System.IO;
using System.Linq;
using Xunit;
using OrderManagement.Models;
using OrderManagement.Services;

namespace OrderManagement.Tests
{
    public class OrderServiceTests : IDisposable
    {
        private readonly OrderService _service;
        private readonly Customer _testCustomer;
        private readonly Product _testProduct;

        public OrderServiceTests()
        {
            _service = new OrderService();
            _testCustomer = new Customer("C001", "张三");
            _testProduct = new Product("P001", "笔记本电脑", 5999m);
        }

        public void Dispose()
        {
            // 清理测试产生的临时文件
            string tempXml = Path.GetTempFileName();
            if (File.Exists(tempXml)) File.Delete(tempXml);
        }

        private Order CreateTestOrder()
        {
            var order = new Order(_testCustomer);
            order.AddDetail(new OrderDetails(_testProduct, 2, _testProduct.Price));
            _service.AddOrder(order);
            return order;
        }

        [Fact]
        public void AddOrder_ValidOrder_ShouldGenerateOrderIdAndAdd()
        {
            var order = new Order(_testCustomer);
            order.AddDetail(new OrderDetails(_testProduct, 1, _testProduct.Price));

            _service.AddOrder(order);

            Assert.NotEmpty(order.OrderId);
            var allOrders = _service.GetAllOrders();
            Assert.Single(allOrders);
            Assert.Equal(order, allOrders[0]);
        }

        [Fact]
        public void AddOrder_WithoutDetails_ShouldThrowOrderException()
        {
            var order = new Order(_testCustomer);
            Assert.Throws<OrderException>(() => _service.AddOrder(order));
        }

        [Fact]
        public void DeleteOrder_ExistingOrder_ShouldRemove()
        {
            var order = CreateTestOrder();
            _service.DeleteOrder(order.OrderId);
            Assert.Empty(_service.GetAllOrders());
        }

        [Fact]
        public void DeleteOrder_NonExistingOrder_ShouldThrow()
        {
            Assert.Throws<OrderException>(() => _service.DeleteOrder("999"));
        }

        [Fact]
        public void UpdateOrder_ExistingOrder_ShouldReplace()
        {
            var original = CreateTestOrder();
            var newCustomer = new Customer("C002", "李四");
            var updated = new Order(newCustomer, DateTime.Now) { OrderId = original.OrderId };
            updated.AddDetail(new OrderDetails(_testProduct, 3, _testProduct.Price));

            _service.UpdateOrder(updated);
            var result = _service.QueryByOrderId(original.OrderId).Single();
            Assert.Equal(newCustomer.Name, result.Customer.Name);
            Assert.Equal(3 * _testProduct.Price, result.TotalAmount);
        }

        [Fact]
        public void UpdateOrder_NonExistingOrder_ShouldThrow()
        {
            var order = new Order(_testCustomer);
            order.AddDetail(new OrderDetails(_testProduct, 1, _testProduct.Price));
            order.OrderId = "999";
            Assert.Throws<OrderException>(() => _service.UpdateOrder(order));
        }

        [Fact]
        public void QueryByOrderId_Existing_ShouldReturnMatched()
        {
            var order = CreateTestOrder();
            var results = _service.QueryByOrderId(order.OrderId);
            Assert.Single(results);
            Assert.Equal(order.OrderId, results[0].OrderId);
        }

        [Fact]
        public void QueryByOrderId_NonExisting_ShouldThrow()
        {
            Assert.Throws<OrderException>(() => _service.QueryByOrderId("xyz"));
        }

        [Fact]
        public void QueryByProduct_ByName_ShouldReturnOrders()
        {
            CreateTestOrder(); // 包含笔记本电脑
            var results = _service.QueryByProduct("笔记本");
            Assert.Single(results);
        }

        [Fact]
        public void QueryByProduct_ByNumber_ShouldReturnOrders()
        {
            CreateTestOrder();
            var results = _service.QueryByProduct("1"); // P001 的编号为 1
            Assert.Single(results);
        }

        [Fact]
        public void QueryByProduct_NotFound_ShouldThrow()
        {
            Assert.Throws<OrderException>(() => _service.QueryByProduct("不存在的商品"));
        }

        [Fact]
        public void QueryByCustomer_ByName_ShouldReturnOrders()
        {
            CreateTestOrder();
            var results = _service.QueryByCustomer("张三");
            Assert.Single(results);
        }

        [Fact]
        public void QueryByCustomer_NotFound_ShouldThrow()
        {
            Assert.Throws<OrderException>(() => _service.QueryByCustomer("王五"));
        }

        [Fact]
        public void QueryByTotalAmount_Filter_ShouldReturnOrdersAboveThreshold()
        {
            CreateTestOrder(); // 总金额 11998
            var results = _service.QueryByTotalAmount(10000);
            Assert.Single(results);
            results = _service.QueryByTotalAmount(20000);
            Assert.Empty(results);
            Assert.Throws<OrderException>(() => _service.QueryByTotalAmount(20000));
        }

        [Fact]
        public void SetSortKey_ShouldChangeOrderOfGetAllOrders()
        {
            var order1 = new Order(new Customer("C001", "A"));
            order1.AddDetail(new OrderDetails(_testProduct, 1, _testProduct.Price)); // 5999
            _service.AddOrder(order1);

            var order2 = new Order(new Customer("C002", "B"));
            order2.AddDetail(new OrderDetails(_testProduct, 2, _testProduct.Price)); // 11998
            _service.AddOrder(order2);

            _service.SetSortKey(o => o.TotalAmount);
            var sorted = _service.GetAllOrders();
            Assert.Equal(5999, sorted[0].TotalAmount);
            Assert.Equal(11998, sorted[1].TotalAmount);

            _service.SetSortKey(o => -o.TotalAmount);
            sorted = _service.GetAllOrders();
            Assert.Equal(11998, sorted[0].TotalAmount);
            Assert.Equal(5999, sorted[1].TotalAmount);
        }

        [Fact]
        public void ResetSort_ShouldRestoreDefaultOrder()
        {
            var order1 = new Order(new Customer("C001", "A"));
            order1.AddDetail(new OrderDetails(_testProduct, 1, _testProduct.Price));
            _service.AddOrder(order1);
            var order2 = new Order(new Customer("C002", "B"));
            order2.AddDetail(new OrderDetails(_testProduct, 2, _testProduct.Price));
            _service.AddOrder(order2);

            _service.SetSortKey(o => -o.TotalAmount);
            _service.ResetSort();
            var sorted = _service.GetAllOrders();
            // 默认按订单号（添加顺序）升序，order1 先添加所以订单号较小
            Assert.Equal(order1.OrderId, sorted[0].OrderId);
            Assert.Equal(order2.OrderId, sorted[1].OrderId);
        }

        [Fact]
        public void Export_ShouldCreateXmlFile()
        {
            CreateTestOrder();
            string tempFile = Path.GetTempFileName();
            try
            {
                _service.Export(tempFile);
                Assert.True(File.Exists(tempFile));
                string content = File.ReadAllText(tempFile);
                Assert.Contains("笔记本电脑", content);
                Assert.Contains("5999", content);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void Import_ShouldReplaceCurrentOrders()
        {
            // 先添加一个订单
            CreateTestOrder();
            Assert.Single(_service.GetAllOrders());

            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfOrder>
  <Order>
    <OrderId>100</OrderId>
    <Customer><CustomerId>C100</CustomerId><Name>王五</Name></Customer>
    <OrderDate>2025-01-01T00:00:00</OrderDate>
    <Details>
      <OrderDetails>
        <Product><ProductId>P002</ProductId><Name>机械键盘</Name><Price>399</Price></Product>
        <Quantity>1</Quantity><UnitPrice>399</UnitPrice>
      </OrderDetails>
    </Details>
  </Order>
</ArrayOfOrder>";
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, xml);

            try
            {
                _service.Import(tempFile);
                var orders = _service.GetAllOrders();
                Assert.Single(orders);
                Assert.Equal("100", orders[0].OrderId);
                Assert.Equal("王五", orders[0].Customer.Name);
                Assert.Equal(399, orders[0].TotalAmount);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void Import_NonExistentFile_ShouldThrow()
        {
            string badFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xml");
            Assert.Throws<OrderException>(() => _service.Import(badFile));
        }
    }
}