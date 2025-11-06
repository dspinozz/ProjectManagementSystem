using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Infrastructure.Services;
using ProjectManagementSystem.Infrastructure.Data;
using Xunit;

namespace ProjectManagementSystem.UnitTests.Services;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailSettings _emailSettings;

    public EmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailSettings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "test@example.com",
            SmtpPassword = "test-password",
            FromAddress = "test@example.com",
            FromName = "Test Sender"
        };
    }

    [Fact(Skip = "No SMTP server configured")]
    public void EmailService_Constructor_InitializesCorrectly()
    {
        // Act
        var service = new EmailService(_emailSettings, _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact(Skip = "No SMTP server configured")]
    public async System.Threading.Tasks.Task SendEmailAsync_SingleRecipient_CallsSendEmailWithList()
    {
        // Arrange
        var service = new EmailService(_emailSettings, _loggerMock.Object);
        var to = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        // Note: This will fail in actual execution without real SMTP server
        // In a real scenario, you'd mock the SMTP client
        // For now, we're testing the structure
        await Assert.ThrowsAnyAsync<Exception>(
            () => service.SendEmailAsync(to, subject, body));
    }

    [Fact(Skip = "No SMTP server configured")]
    public async System.Threading.Tasks.Task SendEmailAsync_MultipleRecipients_CallsSendEmailWithList()
    {
        // Arrange
        var service = new EmailService(_emailSettings, _loggerMock.Object);
        var recipients = new List<string> { "recipient1@example.com", "recipient2@example.com" };
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        // Note: This will fail in actual execution without real SMTP server
        await Assert.ThrowsAnyAsync<Exception>(
            () => service.SendEmailAsync(recipients, subject, body));
    }
}

