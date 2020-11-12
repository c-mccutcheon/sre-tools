using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

namespace web_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async context =>
            {
                var headers = context.Request.Headers;
                var certHeader = headers["X-ARR-ClientCert"];
                if (!String.IsNullOrEmpty(certHeader))
                {
                    try
                    {
                        byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                        var certificate = new X509Certificate2(clientCertBytes);
                        var certSubject = certificate.Subject;
                        var certIssuer = certificate.Issuer;
                        var certThumbprint = certificate.Thumbprint;
                        var certSignatureAlg = certificate.SignatureAlgorithm.FriendlyName;
                        var certIssueDate = certificate.NotBefore.ToShortDateString() + " " + certificate.NotBefore.ToShortTimeString();
                        var certExpiryDate = certificate.NotAfter.ToShortDateString() + " " + certificate.NotAfter.ToShortTimeString();
                        
                        var output = $"MTLS Certificate Presented \n Subject: {certSubject} \n Issuer: {certIssuer} \n Thumbprint: {certThumbprint} \n SignatureAlg: {certSignatureAlg} \n IssueDate {certIssueDate} \n ExpiryDate: {certExpiryDate}";
                        await context.Response.WriteAsync(output);
                    }
                    catch (Exception ex)
                    {
                        var errorString = ex.ToString();
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync(errorString);
                    }
                    finally 
                    {
                        //var isValidCert = IsValidClientCertificate();
                        //if (!isValidCert) context.Response.StatusCode = 403;
                        //else context.Response.StatusCode = 200;
                    }
                }
                else
                {
                    certHeader = "";
                    await context.Response.WriteAsync("No client certificate header presented.");
                }
            });
        }

        private bool IsValidClientCertificate() { return true; }
    }
}
