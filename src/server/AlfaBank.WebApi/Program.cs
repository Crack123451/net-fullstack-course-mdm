// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AlfaBank.WebApi
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Main class.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "For using ReSharper")]
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">all arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Method for create web host builder.
        /// </summary>
        /// <param name="args">all arguments.</param>
        /// <returns><see cref="IWebHostBuilder"/> class.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .Build();

            return WebHost
                .CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .UseStartup<Startup>();
        }
    }
}