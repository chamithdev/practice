using Api.Interface;
using Api.Services;
using DA.Model;
using DA.Model.Data;
using DA.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using DA.Model.Helpers;
using Api.Helpers;

namespace Api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicaitonServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}
