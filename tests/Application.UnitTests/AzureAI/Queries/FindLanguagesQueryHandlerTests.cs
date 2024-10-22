using System.Text.Json;
using AzureAI.Application.AzureAI.Queries.FindLanguages;
using AzureAI.Application.Common.Interfaces;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace AzureAI.Application.UnitTests.AzureAI.Queries;
[TestFixture]
public class FindLanguagesQueryHandlerTests
{
    private Mock<IAzureAIClient> _azureAIClientMock;
    private Mock<IOptions<AISettingsOption>> _optionsMock;
    private Mock<ILogger<FindLanguagesQueryHandler>> _loggerMock;
    private FindLanguagesQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _azureAIClientMock = new Mock<IAzureAIClient>();
        _optionsMock = new Mock<IOptions<AISettingsOption>>();
        _loggerMock = new Mock<ILogger<FindLanguagesQueryHandler>>();

        _optionsMock.Setup(x => x.Value).Returns(new AISettingsOption { ServiceKey = "test-key" });
        _handler = new FindLanguagesQueryHandler(_optionsMock.Object, _azureAIClientMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnResponse_WhenApiCallIsSuccessful()
    {
        // Arrange
        var request = new FindLanguagesQuery { InputText = "Hello world" };
        var responseContent = new FindLanguagesResponse();

        var responseMessage = new HttpResponseMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(responseContent)),
            StatusCode = System.Net.HttpStatusCode.OK
        };

        _azureAIClientMock
            .Setup(x => x.FindLanguage(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        _azureAIClientMock.Verify(x => x.FindLanguage(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnDefaultResponse_WhenApiResponseIsNull()
    {
        // Arrange
        var request = new FindLanguagesQuery { InputText = "Hello world" };

        var responseMessage = new HttpResponseMessage
        {
            Content = null,
            StatusCode = System.Net.HttpStatusCode.OK
        };

        _azureAIClientMock
            .Setup(x => x.FindLanguage(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<FindLanguagesResponse>(result); // Assuming a default instance
    }
}
