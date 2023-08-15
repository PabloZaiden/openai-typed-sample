using OpenAITypedSample;

var samples = new Samples();

await samples.Sample0_AskOpenAIToGenerateList();

await samples.Sample1_AskOpenAIToFormatAsJson();

await samples.Sample1prime_AskOpenAIToFormatAsJsonWithFormat();

//Use chat functions: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai-readme?view=azure-dotnet-preview#use-chat-functions
await samples.Sample2_AskOpenAIToCallAFunction();

await samples.Sample3_AskOpenAIToCallATypedFunction();

await samples.Sample4_AskOpenAIToCallAnAnonymouslyTypedFunction();