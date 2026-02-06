using System;
using System.Collections.Generic;

namespace Sql.Models
{
    /// <summary>
    /// Represents an order entity in the database
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user identifier
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the total order amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Gets or sets the shipping address
        /// </summary>
        public string ShippingAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the collection of order items
        /// </summary>
        public List<OrderItem> OrderItems { get; set; } = new();
    }

    /// <summary>
    /// Represents an order item entity in the database
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the subtotal amount
        /// </summary>
        public decimal Subtotal { get; set; }
    }

    /// <summary>
    /// Enumeration of possible order statuses
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Pending order status
        /// </summary>
        Pending,

        /// <summary>
        /// Confirmed order status
        /// </summary>
        Confirmed,

        /// <summary>
        /// Processing order status
        /// </summary>
        Processing,

        /// <summary>
        /// Shipped order status
        /// </summary>
        Shipped,

        /// <summary>
        /// Completed order status
        /// </summary>
        Completed,

        /// <summary>
        /// Cancelled order status
        /// </summary>
        Cancelled
    }
}