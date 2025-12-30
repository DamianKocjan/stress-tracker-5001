using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StressTracker5001Server.Data;
using StressTracker5001Server.Services;
using StressTracker5001Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IBoardAuthorizationService, BoardAuthorizationService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICardAssignmentService, CardAssignmentService>();
builder.Services.AddScoped<IColumnService, ColumnService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IBoardInviteService, BoardInviteService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

// File Storage Service Configuration
// Use Cloudflare R2 for production
// Use LocalFileStorageService for development/testing
if (builder.Environment.IsProduction())
{
    builder.Services.AddScoped<IFileStorageService, CloudflareFileStorageService>();
}
else
{
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
}

// Email Service Configuration - Use Mock by default for development
var emailServiceType = builder.Configuration.GetValue<string>("EmailService:Type", "Mock");
if (emailServiceType == "Mock")
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    // Future: Add real SMTP service here
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}

// JWT Configuration
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies[builder.Configuration["Jwt:AuthTokenCookieName"]!];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(builder.Configuration["WebApplicationUrl"]!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Use static files only in non-development environments for file uploads
if (!app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
}

// Use exception handler middleware
app.UseExceptionHandler();

app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
