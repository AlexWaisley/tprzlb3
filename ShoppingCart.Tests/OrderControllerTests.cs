using System.Linq.Expressions;
using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Utility;
using ShoppingCart.Web.Areas.Admin.Controllers;
using Stripe;
using Xunit;

namespace ShoppingCart.Tests;

public class OrderControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrderController _orderController;

    public OrderControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderController = new OrderController(_unitOfWorkMock.Object);
    }

    [Fact]
    public void OrderDetails_ReturnsOrderVM()
    {
        // Arrange
        var orderHeader = new OrderHeader { Id = 1 };
        var orderDetails = new[] { new OrderDetail { OrderHeaderId = 1 } }.AsQueryable();
        _unitOfWorkMock.Setup(u =>
                u.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), "ApplicationUser"))
            .Returns(orderHeader);
        _unitOfWorkMock.Setup(u => u.OrderDetail.GetAll("Product")).Returns(orderDetails);

        // Act
        var result = _orderController.OrderDetails(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.OrderHeader.Id);
        Assert.Single(result.OrderDetails);
    }

    [Fact]
    public void SetToInProcess_UpdatesStatus()
    {
        // Arrange
        var orderHeader = new OrderHeader { Id = 1 };
        _unitOfWorkMock.Setup(u => u.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), null))
            .Returns(orderHeader);
        var orderVM = new OrderVM { OrderHeader = orderHeader };

        // Act
        _orderController.SetToInProcess(orderVM);

        // Assert
        _unitOfWorkMock.Verify(u => u.OrderHeader.UpdateStatus(1, OrderStatus.StatusInProcess, null), Times.Once);
        _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
    }


    [Fact]
    public void SetToShipped_UpdatesStatusAndShippingDetails()
    {
        // Arrange
        var orderHeader = new OrderHeader { Id = 1 };
        _unitOfWorkMock.Setup(u => u.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), null))
            .Returns(orderHeader);
        var orderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1, Carrier = "DHL", TrackingNumber = "123" } };

        // Act
        _orderController.SetToShipped(orderVM);

        // Assert
        Assert.Equal("DHL", orderHeader.Carrier);
        Assert.Equal("123", orderHeader.TrackingNumber);
        Assert.Equal(OrderStatus.StatusShipped, orderHeader.OrderStatus);
        _unitOfWorkMock.Verify(u => u.OrderHeader.Update(orderHeader), Times.Once);
        _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
    }
}