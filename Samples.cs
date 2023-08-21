namespace OpenAITypedSample;

using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.AI.OpenAI;
using NJsonSchema;
using NJsonSchema.Generation;

public class Samples
{
    const string GeneralSystemPrompt = "You are an assistant.";
    const string GeneralUserPrompt = "Given the following names, I need to know the quote of the day, which are boy names and which are girl names, and for each group, which is the most popular name: John, Amy, Bob, Alice, Chris, Sarah, Alex, Mary, Steve, Jane, Brian, Lisa";

    OpenAIClient _client;
    string _deploymentName;

    public Samples()
    {
        _client = new OpenAIClient(
            new Uri(Config.Get("OPENAI_ENDPOINT")),
            new Azure.AzureKeyCredential(Config.Get("OPENAI_API_KEY")));
        _deploymentName = Config.Get("OPENAI_CHAT_DEPLOYMENT_NAME");
    }

    public async Task Sample0_AskOpenAIToGenerateList()
    {
        Logger.Header();
        var chat = GenerateChat(
            GeneralSystemPrompt,
            GeneralUserPrompt);

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chat);

        var text = response.Value.Choices[0].Message.Content;

        Logger.Info($"Response from OpenAI: {Environment.NewLine}{text}");
    }

    public async Task Sample1_AskOpenAIToFormatAsJson()
    {
        Logger.Header();

        var chat = GenerateChat(
            GeneralSystemPrompt + Environment.NewLine + "You must always reply using only a valid json object",
            GeneralUserPrompt);

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chat);

        var text = response.Value.Choices[0].Message.Content;

        Logger.Info($"Response from OpenAI: {Environment.NewLine}{text}");
    }

    public async Task Sample1prime_AskOpenAIToFormatAsJsonWithFormat()
    {
        Logger.Header();

        var chat = GenerateChat(
            GeneralSystemPrompt + Environment.NewLine + "You must always reply using only a valid json object, without any text before or after",
            GeneralUserPrompt + Environment.NewLine + @"The response must have the following format:
            {
                ""quote"": ""Sample_Quote"",
                ""boys"": { ""most_popular"": ""Name2"", ""list"": [""Name1"", ""Name2"", ""Name3""] },
                ""girls"": { ""most_popular"": ""Name5"", ""list"": [""Name4"", ""Name5""] }
            }");

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chat);

        var text = response.Value.Choices[0].Message.Content;

        Logger.Info($"Response from OpenAI: {Environment.NewLine}{text}");

        var json = JsonObject.Parse(text);

        if (json == null)
        {
            throw new Exception("Invalid response from OpenAI");
        }

        Logger.Info();
        Logger.Info("Quote of the day: " + json["quote"]!.ToString());
        Logger.Info();

        Logger.Info();
        Logger.Info("Boy names: ");
        Logger.Info();
        json["boys"]!["list"]!.AsArray().ToList().ForEach(x => Logger.Info(x!.ToString()));
        Logger.Info();
        Logger.Info("Most popular boy name: " + json["boys"]!["most_popular"]!.ToString());

        Logger.Info();
        Logger.Info();
        Logger.Info("Girl names: ");
        Logger.Info();
        json["girls"]!["list"]!.AsArray().ToList().ForEach(x => Logger.Info(x!.ToString()));
        Logger.Info();
        Logger.Info("Most popular girl name: " + json["girls"]!["most_popular"]!.ToString());
    }

    public async Task Sample2_AskOpenAIToCallAFunction()
    {
        Logger.Header();

        var chat = GenerateChat(
            GeneralSystemPrompt,
            GeneralUserPrompt);

        var functionName = "OutputByBoyOrGirl";
        chat.Functions.Add(new FunctionDefinition(functionName)
        {
            Parameters = BinaryData.FromObjectAsJson(
                new
                {
                    type = "object",
                    properties = new
                    {
                        quote = new
                        {
                            type = "string"
                        },
                        boys = new
                        {
                            type = "object",
                            properties = new
                            {
                                most_popular = new
                                {
                                    type = "string"
                                },
                                list = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "string"
                                    }
                                }
                            },
                            required = new[] { "most_popular", "list" }
                        },
                        girls = new
                        {
                            type = "object",
                            properties = new
                            {
                                most_popular = new
                                {
                                    type = "string"
                                },
                                list = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "string"
                                    }
                                }
                            },
                            required = new[] { "most_popular", "list" }
                        }
                    },
                    required = new[] { "boys", "girls", "quote" }
                }
            )
        });

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chat);

        var choice = response.Value.Choices[0];

        if (choice.FinishReason == CompletionsFinishReason.FunctionCall &&
            choice.Message.FunctionCall.Name == functionName)
        {
            var functionArguments = choice.Message.FunctionCall.Arguments;
            Logger.Info($"Response from OpenAI: {functionName}({functionArguments})");

            var json = JsonObject.Parse(functionArguments);

            if (json == null)
            {
                throw new Exception("Invalid response from OpenAI");
            }

            Logger.Info();
            Logger.Info("Quote of the day: " + json["quote"]!.ToString());
            Logger.Info();

            Logger.Info();
            Logger.Info("Boy names: ");
            Logger.Info();
            json["boys"]!["list"]!.AsArray().ToList().ForEach(x => Logger.Info(x!.ToString()));
            Logger.Info();
            Logger.Info("Most popular boy name: " + json["boys"]!["most_popular"]!.ToString());

            Logger.Info();
            Logger.Info();
            Logger.Info("Girl names: ");
            Logger.Info();
            json["girls"]!["list"]!.AsArray().ToList().ForEach(x => Logger.Info(x!.ToString()));
            Logger.Info();
            Logger.Info("Most popular girl name: " + json["girls"]!["most_popular"]!.ToString());

        }
        else
        {
            Logger.Error("Unexpected response from OpenAI: " + choice.Message.Content);
        }

    }

    public async Task Sample3_AskOpenAIToCallATypedFunction()
    {
        Logger.Header();

        var chat = GenerateChat(
            GeneralSystemPrompt,
            GeneralUserPrompt);

        var functionName = "OutputByBoyOrGirlWithQuote";
        var function = new FunctionDefinition(functionName);

        var schema = JsonSchema.FromType<BoysGirlsAndQuote>(new JsonSchemaGeneratorSettings()
        {
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull
        });

        function.Parameters = BinaryData.FromString(schema.ToJson());

        chat.Functions.Add(function);

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chat);

        var choice = response.Value.Choices[0];

        if (choice.FinishReason == CompletionsFinishReason.FunctionCall &&
            choice.Message.FunctionCall.Name == functionName)
        {
            var functionArguments = choice.Message.FunctionCall.Arguments;
            Logger.Info($"Response from OpenAI: {functionName}({functionArguments})");

            var typedResponse = JsonSerializer.Deserialize<BoysGirlsAndQuote>(functionArguments)!;

            Logger.Info();
            Logger.Info("Quote of the day: " + typedResponse.quote);
            Logger.Info();

            Logger.Info();
            Logger.Info("Boy names: ");
            foreach (var name in typedResponse.boys.list)
            {
                Logger.Info(name);
            }
            Logger.Info();
            Logger.Info("Most popular boy name: " + typedResponse.boys.most_popular);

            Logger.Info();
            Logger.Info("Girl names: ");
            foreach (var name in typedResponse.girls.list)
            {
                Logger.Info(name);
            }
            Logger.Info();
            Logger.Info("Most popular girl name: " + typedResponse.girls.most_popular);

        }
        else
        {
            Logger.Error("Unexpected response from OpenAI: " + choice.Message.Content);
        }
    }

    public async Task Sample4_AskOpenAIToCallAnAnonymouslyTypedFunction()
    {
        Logger.Header();

        var chat = GenerateChat(
            GeneralSystemPrompt,
            GeneralUserPrompt);

        var functionName = "OutputByBoyOrGirlWithQuote";
        var function = TypedFunctionDefinitionFactory.Create(
            functionName,
            new
            {
                quote = String.Empty,
                boys = new
                {
                    most_popular = String.Empty,
                    list = Array.Empty<string>()
                },
                girls = new
                {
                    most_popular = String.Empty,
                    list = Array.Empty<string>()
                }
            });

        chat.Functions.Add(function);

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chat);

        var choice = response.Value.Choices[0];

        if (choice.FinishReason == CompletionsFinishReason.FunctionCall &&
            choice.Message.FunctionCall.Name == functionName)
        {
            var functionArguments = choice.Message.FunctionCall.Arguments;
            Logger.Info($"Response from OpenAI: {functionName}({functionArguments})");

            var typedResponse = function.ParseResponse(functionArguments);

            Logger.Info();
            Logger.Info("Quote of the day: " + typedResponse.quote);
            Logger.Info();

            Logger.Info();
            Logger.Info("Boy names: ");
            foreach (var name in typedResponse.boys.list)
            {
                Logger.Info(name);
            }
            Logger.Info();
            Logger.Info("Most popular boy name: " + typedResponse.boys.most_popular);

            Logger.Info();
            Logger.Info("Girl names: ");
            foreach (var name in typedResponse.girls.list)
            {
                Logger.Info(name);
            }

            Logger.Info();
            Logger.Info("Most popular girl name: " + typedResponse.girls.most_popular);
        }
        else
        {
            Logger.Error("Unexpected response from OpenAI: " + choice.Message.Content);
        }
    }

    private static ChatCompletionsOptions GenerateChat(string systemPrompt, string userPrompt)
    {
        return new ChatCompletionsOptions(
            new[] {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt)
            });
    }

    record BoysGirlsAndQuote(string quote, People boys, People girls);
    record People(string most_popular, string[] list);
}


public class TypedFunctionDefinition<T> : FunctionDefinition
{
    public TypedFunctionDefinition(string name) : base(name)
    {
        var schema = JsonSchema.FromType<T>(new JsonSchemaGeneratorSettings()
        {
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull
        });

        Parameters = BinaryData.FromString(schema.ToJson());
    }

    public T ParseResponse(string response)
    {
        return JsonSerializer.Deserialize<T>(response)!;
    }
}

public static class TypedFunctionDefinitionFactory
{
    public static TypedFunctionDefinition<T> Create<T>(string name, T parametersTemplate)
    {
        return new TypedFunctionDefinition<T>(name);
    }
}