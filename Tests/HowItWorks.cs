using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Api;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests;

[TestClass]
public class HowItWorks
{
    [TestMethod]
    public async Task ShouldInsertRowsWhenBachIsFullWithoutWaitingForTimer()
    {
        // Arrange
        var actual = new List<Payload>();
        var expected = new List<Payload>
        {
            new Payload() { Id = "1" },
            new Payload() { Id = "2" },
            new Payload() { Id = "3" },
            new Payload() { Id = "4" },
            new Payload() { Id = "5" },
        };
        
        var mock = new Mock<IStorage>();
        mock.Setup(m => m.SaveAsync(It.IsAny<IEnumerable<Payload>>())).Callback<IEnumerable<Payload>>(a => actual = new List<Payload>(a)).Returns(Task.CompletedTask);

        await using var app = new WebApplication(mock.Object);

        
        // Act
        var client = app.CreateClient();
        for (var i = 1; i <= 5; i++)
        {
            var response = await client.GetAsync($"/collect?id={i}");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        Thread.Sleep(100); // tests are faster than background worker, so give it a chance to call mock

        
        // Assert
        mock.Verify(m => m.SaveAsync(It.IsAny<IEnumerable<Payload>>()), Times.Once);
        actual.Should().BeEquivalentTo(expected);
    }
    
    [TestMethod]
    public async Task ShouldInsertRowsWhenTimerToFillBatchIsEnded()
    {
        // Arrange
        var actual = new List<Payload>();
        var expected = new List<Payload>
        {
            new Payload() { Id = "1" }
        };
        
        var mock = new Mock<IStorage>();
        mock.Setup(m => m.SaveAsync(It.IsAny<IEnumerable<Payload>>())).Callback<IEnumerable<Payload>>(a => actual = new List<Payload>(a)).Returns(Task.CompletedTask);

        await using var app = new WebApplication(mock.Object);
        

        // Act
        var client = app.CreateClient();
        var response = await client.GetAsync($"/collect?id=1");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // await client.GetAsync($"/collect?id=2"); // uncomment me to prove the test
        Thread.Sleep(12000); // wait till timer ends

        
        // Assert
        mock.Verify(m => m.SaveAsync(It.IsAny<IEnumerable<Payload>>()), Times.Once);
        actual.Should().BeEquivalentTo(expected);
    }
}