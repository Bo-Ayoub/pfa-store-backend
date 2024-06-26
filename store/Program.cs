using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using store.Helper.Data;
using store.Helper.Db;
using store.Helper.Jwt;
using store.Services.Contract;
using store.Services.Implementation;
using store.Settings;
using Stripe;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IMyApiService, MyApiService>();



//Jwt configuration starts here
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
     };
 });

builder.Services.AddScoped<IDbHelper, db>();

// Chaine De Conx 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StoreDbContext>(options => options.UseSqlServer(connectionString));
// This is just to build the DbContextOptions
//builder.Services.AddDbContext<StoreDbContext>(options =>
//    options.UseSqlServer("Initial connection string"));

//builder.Services.AddDbContext<StoreDbContext>(options => { });


// Auto Mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//add http client
builder.Services.AddHttpClient();


// Register Service
builder.Services.AddScoped<IProductService, store.Services.Implementation.ProductService>();
builder.Services.AddScoped<IClientservice, ClientService>();
builder.Services.AddScoped<IVarianteService, VarianteService>();
builder.Services.AddScoped<IAttVarianteService, AttVarianteService>();
builder.Services.AddScoped<IPhotoVarianteService, PhotoVarianteService>();
builder.Services.AddScoped<ILignePanierService, LignePanierService>();
builder.Services.AddScoped<IPanierService, PanierService>();
builder.Services.AddScoped<IPaiementservice, Paiementservice>();
builder.Services.AddScoped<IRetourservice, Retourservice>();
builder.Services.AddScoped<ICommandService, CommandService>();
builder.Services.AddScoped<ILigneCommandeService, LigneCommandeService>();
builder.Services.AddScoped<IAtt_ProduitService, Att_ProduitService>();
builder.Services.AddScoped<IPhotoProduitService, PhotoProduitService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IHostingEnvironment, HostingEnvironment>();





builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://localhost:4300", "http://localhost:4301", "http://localhost:4302")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed((host) => true)
                ;
    });
});


builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));





var app = builder.Build();

app.UseCors("AllowAll");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure Stripe API Key
var stripeSettings = app.Services.GetRequiredService<IOptions<StripeSettings>>().Value;
StripeConfiguration.ApiKey = stripeSettings.SecretKey;

app.UseAuthorization();

app.MapControllers();

app.Run();
