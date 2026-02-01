# Email Utility Documentation

## Overview
This project uses **MailKit** + **MimeKit** for sending emails. This replaces the deprecated `System.Net.Mail.SmtpClient`.
The implementation provides a clean, async, and testable interface for sending HTML and plain-text emails.

## Components

### 1. `IEmailSender` Interface
Located in `OnlineBookManagementSystem/Interfaces/IEmailSender.cs`.
Defines a single method:
```csharp
Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string? plainTextMessage = null);
```

### 2. `MailKitEmailSender` Implementation
Located in `OnlineBookManagementSystem/Services/MailKitEmailSender.cs`.
- Reads configuration from **Database (cached)**, then **AppSettings**, then **Environment Variables**.
- Uses `IOptions<EmailSettings>` for fallback configuration.
- Supports STARTTLS and SSL.
- Handles exceptions by logging them via `Serilog` and rethrowing to the caller (so the UI can handle the error state).

### 3. `EmailSettings` Model
Located in `OnlineBookManagementSystem/Models/Configuration/EmailSettings.cs`.
Strongly-typed configuration class.

## Configuration

### `appsettings.json`
Add the following section to your `appsettings.json`:

```json
  "EmailSettings": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "your_email@example.com",
    "SmtpPassword": "your_password",
    "EnableSsl": true,
    "SenderName": "Whispering Pages",
    "SenderEmail": "noreply@whisperingpages.com"
  }
```

### Database Configuration
You can also configure these settings via the **System Settings** page in the SuperAdmin dashboard.
Database settings take precedence over `appsettings.json`.

## Usage Examples

### Injecting the Service
```csharp
private readonly IEmailSender _emailSender;

public MyService(IEmailSender emailSender)
{
    _emailSender = emailSender;
}
```

### Sending an Email
```csharp
await _emailSender.SendEmailAsync(
    "user@example.com",
    "Welcome!",
    "<h1>Welcome</h1><p>Thanks for joining.</p>",
    "Welcome! Thanks for joining." // Optional plain text fallback
);
```

## Security Best Practices
- **Never commit passwords to Git.** Use User Secrets or Environment Variables in production.
- **Use App Passwords** if using Gmail or Outlook (Standard passwords often blocked).
- **TLS is enabled by default** (STARTTLS for port 587, Implicit SSL for 465).

## Testing
- Use `Papercut SMTP` or `MailHog` for local development to catch emails without sending them to real addresses.
- The `SystemSettingsService.TestEmailConfigurationAsync` method allows admins to verify SMTP settings from the UI.
