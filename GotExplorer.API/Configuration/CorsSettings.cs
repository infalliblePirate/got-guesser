using Microsoft.AspNetCore.Cors.Infrastructure;

namespace GotExplorer.API.Configuration
{
    public class CorsSettings
    {
        public string[]? Origins { get; set; }
        public string[]? Headers { get; set; }
        public string[]? Methods { get; set; }

        public CorsPolicy GetPolicy()
        {
            CorsPolicyBuilder builder = new CorsPolicyBuilder();
            if (Headers == null || Headers.Length == 0)
                builder.AllowAnyHeader();
            else
                builder.WithHeaders(Headers);


            if (Methods == null || Methods.Length == 0)
                builder.AllowAnyMethod();
            else
                builder.WithMethods(Methods);


            if (Origins == null || Origins.Length == 0)
                builder.AllowAnyOrigin();
            else
                builder.WithOrigins(Origins);
            
            return builder.Build();
        }
    }
}
