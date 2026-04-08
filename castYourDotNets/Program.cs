using System.Text;
using castYourDotNets.Components;
using castYourDotNets.Contracts;
using castYourDotNets.Data;
using castYourDotNets.Models;
using castYourDotNets.Options;
using castYourDotNets.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Fail fast if JWT settings are missing.
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);

var connectionString = builder.Configuration.GetConnectionString("VerseVault")
    ?? $"Data Source={Path.Combine(dataDirectory, "versevault.db")}";

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddDbContext<VerseVaultDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IAccountRepository, EfAccountRepository>();
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddScoped<AccountRegistrationService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AccountApiClient>();
builder.Services.AddScoped<AuthSessionState>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient(nameof(ScriptureService));
builder.Services.AddScoped<ScriptureService>();
// build the api address to pull values for scripture
builder.Services.AddHttpClient<ScriptureService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5076");
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure local schema exists for development; production should use migrations.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VerseVaultDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/api", () => Results.Ok(new
{
    name = "Verse Vault",
    purpose = "Help users create accounts and track scripture memorization progress.",
    includes = new[]
    {
        "Local account creation",
        "Scripture tracking",
        "Memorization progress",
        "Streak support"
    }
}));

app.MapPost("/api/accounts/register", async (
    RegisterAccountRequest request,
    AccountRegistrationService registrationService,
    CancellationToken cancellationToken) =>
{
    var result = await registrationService.RegisterAsync(request, cancellationToken);

    if (!result.Succeeded)
    {
        return Results.ValidationProblem(result.Errors);
    }

    return Results.Created($"/api/accounts/{result.Account!.Id}", result.Account);
});

app.MapPost("/api/accounts/login", async (
    LoginRequest request,
    LoginService loginService,
    CancellationToken cancellationToken) =>
{
    var result = await loginService.LoginAsync(request, cancellationToken);

    if (!result.Succeeded)
    {
        return Results.ValidationProblem(result.Errors);
    }

    return Results.Ok(result.Authentication);
});

app.MapGet("/api/accounts/me", async (
    HttpContext httpContext,
    IAccountRepository accountRepository,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var account = await accountRepository.GetByIdAsync(userId, cancellationToken);

    return account is null
        ? Results.NotFound()
        : Results.Ok(new AccountResponse
        {
            Id = account.Id,
            Username = account.Username,
            CreatedAtUtc = account.CreatedAtUtc
        });
}).RequireAuthorization();

app.MapPost("/api/scriptures", async (
    CreatePageRequest request,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = httpContext.User.GetRequiredUserId();
        var page = new PageClass(
            userId,
            request.Source,
            request.Book,
            request.Chapter,
            request.VerseStart,
            request.VerseEnd,
            request.Text,
            request.Notes);

        dbContext.PageClasses.Add(page);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/scriptures/{page.Id}", ToPageResponse(page));
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = [exception.Message]
        });
    }
}).RequireAuthorization();

app.MapGet("/api/scriptures", async (
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var pages = await dbContext.PageClasses
        .Where(page => page.UserId == userId)
        .ToListAsync(cancellationToken);

    // SQLite cannot order this DateTimeOffset expression server-side, so sort in memory.
    return Results.Ok(pages
        .OrderByDescending(page => page.CreatedAtUtc)
        .Select(ToPageResponse));
}).RequireAuthorization();

app.MapGet("/api/scriptures/{id:guid}", async (
    Guid id,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var page = await dbContext.PageClasses
        .SingleOrDefaultAsync(page => page.Id == id && page.UserId == userId, cancellationToken);

    return page is null ? Results.NotFound() : Results.Ok(ToPageResponse(page));
}).RequireAuthorization();

app.MapPut("/api/scriptures/{id:guid}", async (
    Guid id,
    Scripture request,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var existingPage = await dbContext.PageClasses
        .SingleOrDefaultAsync(page => page.Id == id && page.UserId == userId, cancellationToken);

    if (existingPage is null)
    {
        return Results.NotFound();
    }

    var replacementPage = new PageClass(
        userId,
        string.IsNullOrWhiteSpace(request.ScriptureSource) ? existingPage.Source : request.ScriptureSource,
        request.Book,
        request.Chapter,
        request.VerseNumber,
        request.VerseNumber,
        request.Text,
        existingPage.Notes);

    if (request.IsMemorized)
    {
        replacementPage.MarkMemorized();
    }

    dbContext.PageClasses.Remove(existingPage);
    dbContext.PageClasses.Add(replacementPage);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(ToPageResponse(replacementPage));
}).RequireAuthorization();

app.MapDelete("/api/scriptures/{id:guid}", async (
    Guid id,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var page = await dbContext.PageClasses
        .SingleOrDefaultAsync(page => page.Id == id && page.UserId == userId, cancellationToken);

    if (page is null)
    {
        return Results.NotFound();
    }

    dbContext.PageClasses.Remove(page);
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/api/scriptures/{id:guid}/practice", async (
    Guid id,
    ScripturePracticeRequest request,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var page = await dbContext.PageClasses
        .SingleOrDefaultAsync(page => page.Id == id && page.UserId == userId, cancellationToken);

    if (page is null)
    {
        return Results.NotFound();
    }

    if (request.Succeeded)
    {
        page.RecordReview();
    }

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(ToPageResponse(page));
}).RequireAuthorization();

app.MapPost("/api/scriptures/{id:guid}/memorized", async (
    Guid id,
    ScriptureMemorizedRequest request,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();
    var page = await dbContext.PageClasses
        .SingleOrDefaultAsync(page => page.Id == id && page.UserId == userId, cancellationToken);

    if (page is null)
    {
        return Results.NotFound();
    }

    if (request.IsMemorized)
    {
        page.MarkMemorized();
    }
    else
    {
        page.MarkNotMemorized();
    }

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(ToPageResponse(page));
}).RequireAuthorization();

app.Run();

static PageResponse ToPageResponse(PageClass page) =>
    new()
    {
        Id = page.Id,
        UserId = page.UserId,
        Source = page.Source,
        Book = page.Book,
        Chapter = page.Chapter,
        VerseStart = page.VerseStart,
        VerseEnd = page.VerseEnd,
        Reference = page.Reference,
        Text = page.Text,
        Notes = page.Notes,
        IsMemorized = page.IsMemorized,
        CreatedAtUtc = page.CreatedAtUtc,
        MemorizedAtUtc = page.MemorizedAtUtc,
        ReviewStreakDays = page.ReviewStreakDays,
        LastReviewedAtUtc = page.LastReviewedAtUtc
    };
