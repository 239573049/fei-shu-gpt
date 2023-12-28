using FeiShuGpt.Callers.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region 读取环境变量修改配置

builder.Configuration.GetSection("FeiShuOptions").Get<FeiShuOptions>();
builder.Configuration.GetSection("OpenAIOptions").Get<OpenAiOptions>();

// 读取环境变量
var appid = Environment.GetEnvironmentVariable(Constant.APPID);
var appsecret = Environment.GetEnvironmentVariable(Constant.APPSECRET);
var gptKey = Environment.GetEnvironmentVariable(Constant.GPTKEY);
var gptModel = Environment.GetEnvironmentVariable(Constant.MODEL);
var endpoint = Environment.GetEnvironmentVariable(Constant.ENDPOINT);
var maxHistory = Environment.GetEnvironmentVariable(Constant.MAXHISTORY);
var botName = Environment.GetEnvironmentVariable(Constant.BOTNAME);

if (appid?.IsNullOrEmpty() == false)
    FeiShuOptions.AppId = appid;

if (appsecret?.IsNullOrEmpty() == false)
    FeiShuOptions.AppSecret = appsecret;

if (botName?.IsNullOrEmpty() == false)
    FeiShuOptions.BotName = botName;

if (gptKey?.IsNullOrEmpty() == false)
    OpenAiOptions.Key = gptKey;

if (gptModel?.IsNullOrEmpty() == false)
    OpenAiOptions.Model = gptModel;

if (endpoint?.IsNullOrEmpty() == false)
    OpenAiOptions.Endpoint = endpoint;

if (maxHistory?.IsNullOrEmpty() == false)
    try
    {
        OpenAiOptions.MaxHistory = int.Parse(maxHistory);
    }
    catch
    {
        OpenAiOptions.MaxHistory = 3;
    }

#endregion

builder.Services.AddMasaMinimalAPIs(option => option.MapHttpMethodsForUnmatched = new[] { "Post" });

builder.Services.AddSingleton<IFreeSql>(r =>
{
    return new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=FeiShuGpt.db")
#if DEBUG
        .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}")) //监听SQL语句
#endif
        .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
        .Build();
});

var httpClient = new HttpClient(new OpenAiHttpClientHandler());
httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
httpClient.DefaultRequestHeaders.Add("User-Agent", "FeiShuGpt");
builder.Services.AddTransient<Kernel>((_) => Kernel.CreateBuilder().AddOpenAIChatCompletion(
    OpenAiOptions.Model,
    OpenAiOptions.Key,
    httpClient: httpClient).Build());

builder.Services.AddSingleton((services) => new OpenAIChatCompletionService(OpenAiOptions.Model, OpenAiOptions.Key,
    httpClient: httpClient));

builder.Services.AddCaller(clientBuilder =>
{
    clientBuilder.UseHttpClient()
        .AddMiddleware<TokenCallerMiddleware>();
});

builder.Services.AddAutoRegistrationCaller(new[] { typeof(Program).Assembly }, ServiceLifetime.Singleton);

builder.Services.AddEndpointsApiExplorer()
#if DEBUG
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1",
            new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "FeishuGptApp", Version = "v1",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact { Name = "FeishuGptApp", }
            });
        foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml"))
            options.IncludeXmlComments(item, true);
        options.DocInclusionPredicate((docName, action) => true);
    })
#endif
    ;

var app = builder.Build();

app.Use((async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}));

app.MapMasaMinimalAPIs();

#if DEBUG
app.UseSwagger().UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FeishuGptApp"));
#endif

await app.RunAsync();