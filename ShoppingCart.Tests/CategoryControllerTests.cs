using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Web.Areas.Admin.Controllers;
using Xunit;
using System.Linq.Expressions;
namespace ShoppingCart.Tests;

public class CategoryControllerTests
{
    //GET
    [Fact]
    public void GetCategories_All_ReturnAllCategories()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();

        repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
            .Returns(() => CategoryDataset.Categories);
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act
        var result = controller.Get();

        // Assert
        Assert.Equal(CategoryDataset.Categories, result.Categories);
    }
    
    
    [Fact]
    public void GetCategories_NoCategories_ReturnsEmptyList()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
            .Returns(new List<Category>());
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act
        var result = controller.Get();

        // Assert
        Assert.Empty(result.Categories);
    }
    
    [Fact]
    public void GetCategoryById_ValidId_ReturnsCategory()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var category = CategoryDataset.Categories.First();
        repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
            .Returns(category);
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act
        var result = controller.Get(category.Id);

        // Assert
        Assert.Equal(category, result.Category);
    }
    
    [Fact]
    public void GetCategoryById_InvalidId_ReturnsNull()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act & Assert
        var result = controller.Get(-69);
        Assert.Null(result.Category);
    }
    
    
    
    //POST
    [Fact]
    public void CreateUpdate_ValidCategory_AddsCategory()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var category = new Category { Id = 0, Name = "New Category" };
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act
        controller.CreateUpdate(new CategoryVM { Category = category });

        // Assert
        repositoryMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
    }
    [Fact]
    public void CreateUpdate_ExistingCategory_UpdatesCategory()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var category = new Category { Id = 1, Name = "Updated Category" };
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act
        controller.CreateUpdate(new CategoryVM { Category = category });

        // Assert
        repositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
    }
    
    [Fact]
    public void CreateUpdate_InvalidModelState_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);
        controller.ModelState.AddModelError("Name", "Required");

        // Act & Assert
        Assert.Throws<Exception>(() => controller.CreateUpdate(new CategoryVM()));
    }
    
    //DELETE
    [Fact]
    public void DeleteData_ValidId_DeletesCategory()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var category = CategoryDataset.Categories.First();
        repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
            .Returns(category);
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act
        controller.DeleteData(category.Id);
        // Assert
        repositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Once);
        mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
    }
    
    [Fact]
    public void DeleteData_InvalidId_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act & Assert
        Assert.Throws<Exception>(() => controller.DeleteData(null));
    }
    [Fact]
    public void DeleteData_CategoryNotFound_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<ICategoryRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
        var controller = new CategoryController(mockUnitOfWork.Object);

        // Act & Assert
        Assert.Throws<Exception>(() => controller.DeleteData(999));
    }
}