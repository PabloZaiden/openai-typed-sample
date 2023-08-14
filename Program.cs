// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.AI.OpenAI;
using NJsonSchema;
using NJsonSchema.Generation;
using OpenAITypedSample;

var samples = new Samples();

await samples.Sample0_AskOpenAIToGenerateList();

await samples.Sample1_AskOpenAIToFormatAsJson();

await samples.Sample1prime_AskOpenAIToFormatAsJsonWithFormat();

//Use chat functions: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai-readme?view=azure-dotnet-preview#use-chat-functions
await samples.Sample2_AskOpenAIToCallAFunction();

await samples.Sample3_AskOpenAIToCallATypedFunction();

public class Demo
{
    const string GeneralSystemPrompt = "You are an assistant.";
    const string GeneralUserPrompt = "Given the following names, I need to know which are boy names and which are girl names: John, Amy, Bob, Alice, Chris, Sarah, Alex, Mary, Steve, Jane, Brian, Lisa";

    
    # region Utils
    OpenAIClient _client;
    string _deploymentName;


    public Demo()
    {
        _client = new OpenAIClient(
            new Uri(Config.Get("OPENAI_ENDPOINT")), 
            new Azure.AzureKeyCredential(Config.Get("OPENAI_API_KEY")));
        _deploymentName = Config.Get("OPENAI_CHAT_DEPLOYMENT_NAME");
    }
    

    private static ChatCompletionsOptions GenerateChat(string systemPrompt, string userPrompt)
    {
        return new ChatCompletionsOptions(
            new [] {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt)
            });
    }
    #endregion
}
