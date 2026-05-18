
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implements;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implements;
using PRN232.LMS.Services.Interfaces;
using System.Text.Json.Serialization;

namespace PRN232.LMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<LMSDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()));

            builder.Services.AddScoped(typeof(Repositories.Interfaces.IGenericRepository<>), typeof(Repositories.Implements.IGenericRepository<>));
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<ISemesterService, SemesterService>();
            builder.Services.AddScoped<ISubjectService, SubjectService>();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            for (var attempt = 1; attempt <= 10; attempt++)
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<LMSDbContext>();
                    dbContext.Database.Migrate();
                    break;
                }
                catch when (attempt < 10)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }

            app.Run();
        }
    }
}
