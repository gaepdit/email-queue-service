# Email Batch Queue Application

This application creates an API to queue and process bulk emails as well as a web application to view the status of each
email batch.

A batch of emails can be sent as an array to the API, which will then queue and process them with a configurable delay
between each. The submitted emails are also saved in a database. If the application needs to restart before all emails
are processed, any unsent emails will be loaded from the database and processing will continue.

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

All API endpoints require authentication using an API Key passed in the `X-API-Key` header with each request. Valid API
keys are configured in `appsettings.json`.

```json
{
  "ApiKeys": [
    {
      "owner": "Your Web Application",
      "key": "your-secret-api-key-1"
    }
  ]
}
```

The owner field is saved in the database along with each email submitted using that API Key.

### API Endpoints

#### POST /add

Submits a batch of email tasks for processing.

Request body: Array of email tasks.

```json
[
  {
    "recipients": [
      "email@example.com"
    ],
    "copyRecipients": [],
    "from": "from.email@example.net",
    "subject": "Email Subject",
    "body": "Email content",
    "isHtml": false
  }
]
```

Each email task contains the following properties:

- `recipients`: List of recipient email addresses (Required; may not be empty or contain any empty values)
- `copyRecipients`: List of copied email addresses (Optional; may not contain empty values if included)
- `from`: The return (from) email address (Required)
- `subject`: Email subject line, max 200 characters (Required)
- `body`: Email content, max 20,000 characters (Required)
- `isHtml`: Boolean indicating if the body is formatted as HTML (Required)

Response if successful:

```json
{
  "status": "Success",
  "count": 1,
  "batchId": "id-of-batch"
}
```

Currently, the Batch ID is a ten-character random string.

If no email tasks are submitted, the following response will be returned:

```json
{
  "status": "Empty",
  "count": 0,
  "batchId": ""
}
```

#### GET /batches

Returns a list of all Batch IDs in the system for the provided API Key, ordered by creation date descending.

#### POST /batch/

Body: "{batchId}"

Returns all email tasks for a specific Batch ID, ordered by creation date ascending.

---

## Sample Web App Configuration

A sample web application is provided to demonstrate displaying data from the API. The web application is configured
through `appsettings.json` with the following sections:

### Email Queue API

```json
{
  "EmailQueueApi": {
    "BaseUrl": "https://localhost:7145",
    "ApiKey": "your-secret-api-key-1"
  }
}
```

### Authentication

The sample web application supports two authentication modes:

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
