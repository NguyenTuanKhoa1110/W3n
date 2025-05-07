	using Microsoft.AspNetCore.Identity;
	using Microsoft.EntityFrameworkCore;
	using W3_test.Data;
	using W3_test.Domain.Models;
	using W3_test.Repositories;

	var builder = WebApplication.CreateBuilder(args);

	// Đăng ký DbContext
	builder.Services.AddDbContext<AppDbContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

	// Đăng ký Identity
	builder.Services.AddIdentity<AppUser, AppRole>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequiredLength = 6;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = false;
		options.Password.RequireLowercase = false;
	})
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

	// Đăng ký repository
	builder.Services.AddTransient<IUserRepository, UserRepository>();
	builder.Services.AddTransient<IOrderRepository, OrderRepository>();
	builder.Services.AddTransient<ICartRepository, CartRepository>();
	builder.Services.AddTransient<IBookRepository, BookRepository>();

	// Cấu hình MVC
	builder.Services.AddControllersWithViews();
	builder.Services.AddAutoMapper(typeof(Program));
var app = builder.Build();

	// Thực hiện seed dữ liệu (roles và admin user)
	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;
		var userManager = services.GetRequiredService<UserManager<AppUser>>();
		var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

		await SeedData.SeedRolesAsync(roleManager);  
		await SeedData.SeedAdminAsync(userManager, roleManager);  
	}

	if (!app.Environment.IsDevelopment())
	{
		app.UseExceptionHandler("/Home/Error");
		app.UseHsts();
	}

	app.UseHttpsRedirection();
	app.UseStaticFiles();

	app.UseRouting();

	app.UseAuthentication();
	app.UseAuthorization();

	app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");

	app.Run();
