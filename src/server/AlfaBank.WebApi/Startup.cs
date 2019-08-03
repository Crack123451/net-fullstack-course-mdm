// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AlfaBank.WebApi
{
    using System.Diagnostics.CodeAnalysis;
    using AlfaBank.Core.Models;
    using AlfaBank.Services;
    using AlfaBank.WebApi.Middleware;
    using AutoMapper;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// This class start this program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Service configure.
        /// </summary>
        /// <param name="services">services.</param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(conf => conf.AddProfile<DomainToDtoProfile>());
            services.AddAlfaBankServices();
            services.AddInMemoryUserStorage();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        /// <summary>
        /// Method configure.
        /// </summary>
        /// <param name="app">app.</param>
        /// <param name="env">env.</param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpStatusCodeExceptionMiddleware();

            app.UseMvc();
        }
    }
}