using ECommerceAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ECommerceAPI.Models;
using System.Text;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Services.Interfaces;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddScoped<IStripeService, ECommerceAPI.Services.Implementations.StripeService>();
            builder.Services.AddScoped<IEmailService, ECommerceAPI.Services.Implementations.ResendEmailService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<ICheckoutService, ECommerceAPI.Services.Implementations.CheckoutService>();
            builder.Services.AddScoped<IPaymentService, ECommerceAPI.Services.Implementations.PaymentService>();
            builder.Services.AddScoped<IRecommendationService, ECommerceAPI.Services.Implementations.RecommendationService>();
            builder.Services.AddScoped<IOrderService, ECommerceAPI.Services.Implementations.OrderService>();
            builder.Services.AddScoped<ICartService, ECommerceAPI.Services.Implementations.CartService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ECommerceAPI.Services.Implementations.ProductService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, ECommerceAPI.Services.Implementations.CategoryService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, ECommerceAPI.Services.Implementations.AuthService>();
            builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ICloudinaryService, ECommerceAPI.Services.Implementations.CloudinaryService>();

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            var app = builder.Build();

            Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseCors("AllowAngular");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}