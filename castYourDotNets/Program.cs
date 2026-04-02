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
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using castYourDotNets;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ProtectedSessionStorage>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

// Ensure local schema exists for development; production should use migrations.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VerseVaultDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    //insert the values into database. if empty
    if (!dbContext.VerseVaults.Any())
    {
        SeedData.Initialize(dbContext, builder.Environment.ContentRootPath);
        Console.WriteLine("Verse vault seeded successfully.");
    }
    // creating the local schema have a database for the uneditable (after inital seed) of scripture_verses
    await dbContext.Database.ExecuteSqlRawAsync(
            """
        CREATE TABLE IF NOT EXISTS Scripture_Verses (
            Id TEXT NOT NULL PRIMARY KEY,
            Scripture TEXT NOT NULL,
            Book TEXT NOT NULL,
            Chapter INT NOT NULL,
            VerseInt INT NOT NULL,
            VerseText TEXT NOT NULL
        );
        """);


    await dbContext.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS Scriptures (
            Id TEXT NOT NULL PRIMARY KEY,
            Reference TEXT NOT NULL,
            Text TEXT NOT NULL,
            Topic TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL,
            IsMemorized INTEGER NOT NULL,
            PracticeCount INTEGER NOT NULL,
            CurrentStreakDays INTEGER NOT NULL,
            LastPracticedAtUtc TEXT NULL,
            MemorizedAtUtc TEXT NULL
        );
        """);

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS MemorizationEntries (
            Id TEXT NOT NULL PRIMARY KEY,
            UserId TEXT NOT NULL,
            GameText TEXT NOT NULL,
            IsMemorized INTEGER NOT NULL DEFAULT 0,
            IsMemorizedThroughGame INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (UserId) REFERENCES UserAccounts(Id) ON DELETE CASCADE
        );
        """);
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

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

app.MapGet("/api/scriptures", async (
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scriptures = await dbContext.Scriptures
        .OrderBy(scripture => scripture.Reference)
        .ToListAsync(cancellationToken);

    return Results.Ok(scriptures);
});

app.MapGet("/api/scriptures/{id:guid}", async (
    Guid id,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = await dbContext.Scriptures.FindAsync([id], cancellationToken);
    return scripture is null ? Results.NotFound() : Results.Ok(scripture);
});

app.MapPost("/api/scriptures", async (
    Scripture request,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = new Scripture
    {
        Reference = request.Reference,
        Text = request.Text,
        Topic = request.Topic,
        CreatedAtUtc = DateTime.UtcNow,
        IsMemorized = request.IsMemorized,
        PracticeCount = request.PracticeCount,
        CurrentStreakDays = request.CurrentStreakDays,
        LastPracticedAtUtc = request.LastPracticedAtUtc,
        MemorizedAtUtc = request.MemorizedAtUtc
    };

    dbContext.Scriptures.Add(scripture);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/scriptures/{scripture.Id}", scripture);
});

app.MapPut("/api/scriptures/{id:guid}", async (
    Guid id,
    Scripture request,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = await dbContext.Scriptures.FindAsync([id], cancellationToken);
    if (scripture is null)
    {
        return Results.NotFound();
    }

    scripture.Reference = request.Reference;
    scripture.Text = request.Text;
    scripture.Topic = request.Topic;
    scripture.IsMemorized = request.IsMemorized;
    scripture.PracticeCount = request.PracticeCount;
    scripture.CurrentStreakDays = request.CurrentStreakDays;
    scripture.LastPracticedAtUtc = request.LastPracticedAtUtc;
    scripture.MemorizedAtUtc = request.MemorizedAtUtc;

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(scripture);
});

app.MapDelete("/api/scriptures/{id:guid}", async (
    Guid id,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = await dbContext.Scriptures.FindAsync([id], cancellationToken);
    if (scripture is null)
    {
        return Results.NotFound();
    }

    dbContext.Scriptures.Remove(scripture);
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
});

app.MapPost("/api/scriptures/{id:guid}/practice", async (
    Guid id,
    ScripturePracticeRequest request,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = await dbContext.Scriptures.FindAsync([id], cancellationToken);
    if (scripture is null)
    {
        return Results.NotFound();
    }

    scripture.PracticeCount += 1;

    var now = DateTime.UtcNow;
    if (scripture.LastPracticedAtUtc.HasValue)
    {
        var gap = (now.Date - scripture.LastPracticedAtUtc.Value.Date).Days;
        scripture.CurrentStreakDays = request.Succeeded
            ? (gap == 1 ? scripture.CurrentStreakDays + 1 : 1)
            : 0;
    }
    else
    {
        scripture.CurrentStreakDays = request.Succeeded ? 1 : 0;
    }

    scripture.LastPracticedAtUtc = now;

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(scripture);
});

app.MapPost("/api/scriptures/{id:guid}/memorized", async (
    Guid id,
    ScriptureMemorizedRequest request,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = await dbContext.Scriptures.FindAsync([id], cancellationToken);
    if (scripture is null)
    {
        return Results.NotFound();
    }

    scripture.IsMemorized = request.IsMemorized;
    scripture.MemorizedAtUtc = request.IsMemorized ? DateTime.UtcNow : null;

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(scripture);
});

app.MapPost("/api/pages", async (
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

app.MapPost("/api/memorization", async (
    CreateMemorizationEntryRequest request,
    HttpContext httpContext,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userId = httpContext.User.GetRequiredUserId();

    var entry = new MemorizationEntry
    {
        UserId = userId,
        GameText = request.GameText,
        IsMemorized = request.IsMemorized,
        IsMemorizedThroughGame = request.IsMemorizedThroughGame
    };

    dbContext.MemorizationEntries.Add(entry);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/memorization/{entry.Id}", ToMemorizationEntryResponse(entry));
}).RequireAuthorization();

app.MapGet("/api/memorization/scripture/{id:guid}", async (
    Guid id,
    VerseVaultDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scripture = await dbContext.Scriptures.FindAsync([id], cancellationToken);
    return scripture is null ? Results.NotFound() : Results.Ok(scripture);
}).RequireAuthorization();

app.MapGet("/api/pages", async (
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

app.MapGet("/debug/verses", async (VerseVaultDbContext db) =>
{
    var count = await db.VerseVaults.CountAsync();
    return Results.Ok(new { verseCount = count });
});
app.MapGet("/api/versevault", async (VerseVaultDbContext dbContext) =>
{
    var allVerses = await dbContext.VerseVaults
        .OrderBy(v => v.scripture)
        .ThenBy(v => v.book)
        .ThenBy(v => v.Chapter)
        .ThenBy(v => v.VerseInt)
        .ToListAsync();

    return Results.Ok(allVerses);
});

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

static MemorizationEntryResponse ToMemorizationEntryResponse(MemorizationEntry entry) =>
    new()
    {
        Id = entry.Id,
        UserId = entry.UserId,
        GameText = entry.GameText,
        IsMemorized = entry.IsMemorized,
        IsMemorizedThroughGame = entry.IsMemorizedThroughGame
    };
