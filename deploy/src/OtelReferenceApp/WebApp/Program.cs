using Observability;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var serviceName = "WeatherForecast.WebApp";
            string sourceName = "WebApp";
            builder.Services.AddObservability(serviceName, sourceName, builder.Configuration);
            builder.AddSerilog(serviceName, builder.Configuration);
            builder.Services.AddHttpClient("WebApi", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["WEBAPI_URL"] ?? throw new InvalidOperationException("WEBAPI_URL configuration is missing or empty."));
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapObservability();
            app.Run();
        }
    }
}