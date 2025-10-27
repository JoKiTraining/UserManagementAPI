using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MiddlewareLogging
{
    public class ReqResLogger
    {
        private readonly RequestDelegate _next;
        private readonly string _logFilePath = "Logs/request_response_log.txt";

        public ReqResLogger(RequestDelegate next)
        {
            _next = next;

            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir!);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var method = context.Request.Method;
            var path = context.Request.Path;

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context); // Call next middleware

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var statusCode = context.Response.StatusCode;
            var statusText = ((HttpStatusCode)statusCode).ToString();

            var logLine = $"[{timestamp}] {method} {path} => {statusCode} {statusText}";

            if (statusCode >= 400 && !string.IsNullOrWhiteSpace(responseText))
            {
                logLine += $" | Response: {responseText.Replace(Environment.NewLine, " ")}";
            }

            logLine += Environment.NewLine;

            await File.AppendAllTextAsync(_logFilePath, logLine);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}