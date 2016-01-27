// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Threading.Tasks;
using Benchmarks.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Benchmarks
{
    public class MultipleQueriesEfMiddleware
    {
        private static readonly PathString _path = new PathString(Scenarios.GetPaths(s => s.DbMultiQueryEf)[0]);
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private readonly RequestDelegate _next;

        public MultipleQueriesEfMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments(_path, StringComparison.Ordinal))
            {
                var db = (ApplicationDbContext)httpContext.RequestServices.GetService(typeof(ApplicationDbContext));

                var count = MiddlewareHelpers.GetMultipleQueriesQueryCount(httpContext);
                var rows = await EfDb.LoadMultipleQueriesRows(count, db);

                var result = JsonConvert.SerializeObject(rows, _jsonSettings);

                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.ContentLength = result.Length;

                await httpContext.Response.WriteAsync(result);

                return;
            }

            await _next(httpContext);
        }
    }

    public static class MultipleQueriesEfMiddlewareExtensions
    {
        public static IApplicationBuilder UseMultipleQueriesEf(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MultipleQueriesEfMiddleware>();
        }
    }
}
