namespace KalolCommunity.Api.Middleware
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has a body
            if (context.Request.ContentLength > 0)
            {
                // Enable buffering to allow reading the request body multiple times
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset the stream position for further processing
                // Validate the request body (you can implement your own validation logic here)
                if (string.IsNullOrWhiteSpace(body))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Request body cannot be empty.");
                    return;
                }
            }
            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
