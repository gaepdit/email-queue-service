# Email Batch Queue Application

This application creates an API to queue and process bulk emails as well as a web application to view the status of each
email batch.

A batch of emails can be sent as an array to the API, which will then queue and process them with a configurable delay
between each. The emails are saved in a database with their current status. If the application needs to restart before
all emails are processed, any unsent emails will be loaded from the database and processing will continue.

## API Configuration

The API application is configured through `appsettings.json` with the following sections:

### Email Queue Settings

Configure the delay in seconds between processing each email.

```json
{
  "EmailQueueSettings": {
    "ProcessingDelaySeconds": 5
  }
}
```

### Security

All API endpoints require authentication using an API key passed in the `X-API-Key` header with each request. Valid API
keys are configured in `appsettings.json`.

```json
{
  "ApiKeys": [
    {
      "key": "your-secret-api-key-1",
      "owner": "Your Web Application"
    }
  ]
}
```

### Endpoints

#### POST /emailTasks

Enqueues a batch of email tasks for processing.

Request body: Array of email tasks

```json
[
  {
    "recipients": [
      "email@example.com"
    ],
    "from": "from.email@example.net",
    "subject": "Email Subject",
    "body": "Email content",
    "isHtml": false
  }
]
```

Each email task contains the following properties:

- `recipients`: List of email addresses (Required)
- `from`: The return (from) email address (Required, but may be empty. If empty, the `DefaultSenderEmail` address from
  the `EmailServiceSettings` is used)
- `subject`: Email subject line, max 200 characters (Required)
- `body`: Email content, max 20,000 characters (Required)
- `isHtml`: Boolean indicating if the body is formatted as HTML (Required)

Response if successful:

```json
{
  "status": "Success",
  "count": 1,
  "batchId": "guid-of-batch"
}
```

If no email tasks are submitted, the following response will be returned:

```json
{
  "status": "Empty",
  "count": 0,
  "batchId": ""
}
```

#### GET /emailTasks/list

Returns all batch IDs in the system, ordered by creation date descending.

#### GET /emailTasks/list/{batchId}

Returns all email tasks for a specific batch ID, ordered by creation date ascending.

---

## Web App Configuration

The web application is configured through `appsettings.json` with the following sections:

### Email Queue API Connection

Configure the base URL and API key for connecting to the Email Queue API.

```json
{
  "EmailQueueApi": {
    "BaseUrl": "https://localhost:7145",
    "ApiKey": "your-secret-api-key-1"
  }
}
```

### Authentication

The application supports two authentication modes:

* Dev/fake authentication (default when running in a development environment)

  To test authentication failure when using the dev authentication, set `DevAuthFails` to `true`.

* Microsoft Entra ID authentication (default when running in a production environment)

  To enable Entra ID authentication while running in development, set `UseEntraId` to `true`.

```json
{
  "UseEntraId": false,
  "DevAuthFails": false,
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "CallbackPath": "/signin-oidc",
    "Domain": "[Enter the domain of your tenant]",
    "TenantId": "[Enter the Directory (tenant) ID]",
    "ClientId": "[Enter the Application (client) ID]"
  }
}
```
