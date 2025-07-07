var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddGrpcClient<AuthService.Protos.AuthService.AuthServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Auth"]!));
builder.Services.AddGrpcClient<VehicleService.Protos.VehicleService.VehicleServiceClient>(o =>
    o.Address = new Uri(builder.Configuration["Services:Vehicle"]!));

var app = builder.Build();

app.MapGet("/", () => "API Gateway running");

// Auth REST endpoints
app.MapPost("/auth/login", async (AuthService.Protos.AuthService.AuthServiceClient client, AuthService.Protos.LoginRequest request) =>
{
    try
    {
        var reply = await client.LoginAsync(request);
        return Results.Ok(reply);
    }
    catch (RpcException ex)
    {
        return Results.Problem(ex.Status.Detail);
    }
});

app.MapPost("/auth/register", async (AuthService.Protos.AuthService.AuthServiceClient client, AuthService.Protos.RegisterRequest request) =>
{
    try
    {
        var reply = await client.RegisterAsync(request);
        return Results.Ok(reply);
    }
    catch (RpcException ex)
    {
        return Results.Problem(ex.Status.Detail);
    }
});

// Vehicle REST endpoints
app.MapGet("/vehicle", async (VehicleService.Protos.VehicleService.VehicleServiceClient client) =>
{
    try
    {
        var reply = await client.ObtenerTodosVehiculosAsync(new Google.Protobuf.WellKnownTypes.Empty());
        return Results.Ok(reply);
    }
    catch (RpcException ex)
    {
        return Results.Problem(ex.Status.Detail);
    }
});

app.MapGet("/vehicle/{id:int}", async (int id, VehicleService.Protos.VehicleService.VehicleServiceClient client) =>
{
    try
    {
        var request = new VehicleService.Protos.ObtenerVehiculoPorIdRequest { VehiculoId = id };
        var reply = await client.ObtenerVehiculoPorIdAsync(request);
        return Results.Ok(reply);
    }
    catch (RpcException ex)
    {
        return Results.Problem(ex.Status.Detail);
    }
});

app.MapReverseProxy();

app.Run();
