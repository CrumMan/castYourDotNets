using System.Text;
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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
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
