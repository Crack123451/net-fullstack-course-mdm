// <copyright file="MiddlewareExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AlfaBank.WebApi.Middleware
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Middleware for extensions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Method for use http status code.
        /// </summary>
        /// <param name="builder">builder.</param>
        /// <returns><see cref="IApplicationBuilder"/> class.</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IApplicationBuilder UseHttpStatusCodeExceptionMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<HttpStatusCodeExceptionMiddleware>();

        /// <summary>
        /// Method for write error.
        /// </summary>
        /// <param name="context">context.</param>
        /// <param name="statusCode">status code.</param>
        /// <param name="message">message.</param>
        /// <returns><see cref="Task"/> class.</returns>
        public static async Task WriteErrorAsync(this HttpContext context, int statusCode, string message)
        {
            var error = new
            {
                httpStatusCode = statusCode,
                errorMessage = message,
            };
            var json = JsonConvert.SerializeObject(error);

            await context.Response.WriteAsync(json);
        }
    }
}