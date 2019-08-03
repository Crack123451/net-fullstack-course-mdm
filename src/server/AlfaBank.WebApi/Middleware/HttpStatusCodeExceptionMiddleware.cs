// <copyright file="HttpStatusCodeExceptionMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AlfaBank.WebApi.Middleware
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using AlfaBank.Core.Exceptions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Middleware exception for determine status code.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "For using ReSharper")]
    [ExcludeFromCodeCoverage]
    public class HttpStatusCodeExceptionMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeExceptionMiddleware"/> class.
        /// Constructor for this class.
        /// </summary>
        /// <param name="next">next delegate.</param>
        public HttpStatusCodeExceptionMiddleware(RequestDelegate next)
        {
            this.next = next ??
                    throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Main method in middleware.
        /// </summary>
        /// <param name="context">Get context.</param>
        /// <returns><see cref="Task"/> class.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (CriticalException ex)
            {
                var code = (int) ex.StatusCode;

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = code;
                }

                await context.WriteErrorAsync(code, "500 Critical server error - " + ex.Message);
            }
            catch (Exception ex)
            {
                const int code = (int) HttpStatusCode.InternalServerError;

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = code;
                }

                await context.WriteErrorAsync(code, "500 Critical server error - " + ex.Message);
            }
        }
    }
}