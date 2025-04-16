# Email Queue API

This application creates an API to queue and process bulk emails. 

Email tasks can be sent as an array to the API, which will then queue and process each in order with a configurable delay between each. The emails and status are saved in a database. If the application needs to restart before all emails are processed, any unsent emails will be loaded from the database and processing will continue.

## Configuration

The application is configured through `appsettings.json` with the following sections:

### Email Queue Settings

Configure the delay in seconds between processing each email.

```json
"EmailQueueSettings": {
    "ProcessingDelaySeconds": 5 
}
```

## Security

All API endpoints require authentication using an API key passed in the `X-API-Key` header with each request. Valid API keys are configured in `appsettings.json`.

```json
"ApiKeys": [
    {
        "key": "your-secret-api-key-1",
        "owner": "Application One",
        "generatedAt": "2025-04-15T00:00:00Z"
    }
]
```

## API Endpoints

### POST /emailTasks

Enqueues a batch of email tasks for processing.

Request body: Array of email tasks
```json
[
  {
    "recipients": [
        "email@example.com"
    ],
    "subject": "Email Subject",
    "body": "Email content",
    "isHtml": false
  }
]
```

Response:
```json
{
    "status": "Success",
    "message": "Emails have been queued.",
    "count": 1,
    "batchId": "guid-of-batch"
}
```

### GET /emailTasks/list
Returns all email tasks in the system, ordered by creation date descending.

### GET /emailTasks/list/{batchId}
Returns all email tasks for a specific batch ID, ordered by creation date ascending.

## Email Task Properties

- `recipients`: List of email addresses (Required)
- `subject`: Email subject line, max 200 characters (Required)
- `body`: Email content, max 20,000 characters (Required)
- `isHtml`: Boolean indicating if the body is formatted as HTML (Optional, defaults to false)

