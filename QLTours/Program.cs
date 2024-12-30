using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using QLTours.Data;
using QLTours.Models;
using QLTours.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ cho việc tiêm phụ thuộc
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ImageTourService>();

builder.Services.AddScoped<TourDAO>();
builder.Services.AddScoped<UserDAO>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BookingDAO>();
builder.Services.AddScoped<ContactDAO>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddAuthorization();

// Cấu hình dịch vụ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// Thêm dịch vụ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
	{
		Title = "API Đặt Tour",
		Version = "v1",
		Description = "API hỗ trợ quản lý và đặt tour du lịch"
	});
});

// Cấu hình Authentication sử dụng Cookie và Google
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/Account/Login";
	options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(googleOptions =>
{
	googleOptions.ClientId = builder.Configuration["Google:ClientId"];
	googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"];
});

// Cấu hình kết nối đến cơ sở dữ liệu SQL Server
builder.Services.AddDbContext<QuanLyTourContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("QLTour"));
});

// Cấu hình CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

// Cấu hình các dịch vụ khác (Controllers, JSON, v.v.)
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
	options.JsonSerializerOptions.MaxDepth = 64;
});

var app = builder.Build();

// Cấu hình môi trường phát triển để sử dụng Swagger UI
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Đặt Tour v1");
		c.RoutePrefix = string.Empty;
	});
}

// Cấu hình các middleware khác
app.UseCors("AllowAll");
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


// Thêm middleware session
app.UseSession();

// Định tuyến cho API controllers
app.MapControllers();

// Chạy ứng dụng
app.Run();
