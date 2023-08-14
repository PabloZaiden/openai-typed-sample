# OpenAI typed function invocation

This sample shows how to make a request to the Azure OpenAI service and receive a typed response, using well-known or anonymous types.

## How to run

Create a `.env` file in the root directory of the sample with the following contents:

```bash
OPENAI_ENDPOINT="https://<your_endpoint>.openai.azure.com/"
OPENAI_API_KEY="..."
OPENAI_CHAT_DEPLOYMENT_NAME="your-deployment-name"

LOG_LEVEL="Debug"
```

Run the sample with the following command:

```bash
dotnet run
```
