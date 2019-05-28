﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using SamlCore.AspNetCore.Authentication.Saml2;
using SamlCore.AspNetCore.Authentication.Saml2.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Saml2Authentication
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
            services.AddHttpContextAccessor(); //add for Saml2Core
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });          

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddSamlCore(options =>
            {
                // SignOutPath (REQUIRED) - The endpoint for the idp to perform its signout action
                options.SignOutPath = "/signedout";

                // EntityId (REQUIRED) - The Relying Party Identifier e.g. https://my.la.gov.local
                options.ServiceProvider.EntityId = Configuration["AppConfiguration:ServiceProvider:EntityId"];

                // There are two ways to provide FederationMetadata
                // Option 1 - A FederationMetadata.xml already exists for your application
                // options.MetadataAddress = @"FederationMetadata.xml";

                // Option 2 - Have the middleware generate the FederationMetadata.xml file for you
                options.MetadataAddress = Configuration["AppConfiguration:IdentityProvider:MetadataAddress"];

                options.RequireMessageSigned = false;
                options.WantAssertionsSigned = true;

                // Have the middleware create the metadata file for you
                // The default is false. If you don't want a file generated by the middleware, comment the line below.
                options.CreateMetadataFile = true;

                // If you want to specify the filename and path for the generated metadata file do so below:
                //options.DefaultMetadataFileName = "MyMetadataFilename.xml";
                //options.DefaultMetadataFolderLocation = "MyPath";

                // (REQUIRED IF) signing AuthnRequest with Sp certificate to Idp. The value here is the certifcate serial number.
                //options.ServiceProvider.SigningCertificateX509TypeValue = Configuration["AppConfiguration:ServiceProvider:CertificateSerialNumber"]; //your certifcate serial number (default type which can be chnaged by ) that is in your certficate store
                //options.ServiceProvider.CertificateStoreName = StoreName.My;
                //optional if you want the search for the Sp certificate by somethign else other than SerialNumber. The default is serial number. 
                //options.ServiceProvider.CertificateIdentifierType = X509FindType.FindBySerialNumber; // the default is 'X509FindType.FindBySerialNumber'. Change value of 'options.ServiceProvider.SigningCertificateX509TypeValue' if this changes

                // Force Authentication (optional) - Is authentication required?
                options.ForceAuthn = true;
                options.WantAssertionsSigned = false;
                options.RequireMessageSigned = true;
                options.ServiceProvider.X509Certificate2 = new X509Certificate2(@"democert.pfx", "1234");
                //if you want to search in cert store
                //options.ServiceProvider.X509Certificate2 = X509Certificate2Ext.GetX509Certificate2(
                //    Configuration["AppConfiguration:ServiceProvider:CertificateSerialNumber"],
                //    StoreName.My,
                //    StoreLocation.LocalMachine,
                //    X509FindType.FindBySerialNumber
                //    );

                // Service Provider Properties (optional) - These set the appropriate tags in the metadata.xml file
                options.ServiceProvider.ServiceName = "My Test Site";
                options.ServiceProvider.Language = "en-US";
                options.ServiceProvider.OrganizationDisplayName = "Louisiana State Government";
                options.ServiceProvider.OrganizationName = "Louisiana State Government";
                options.ServiceProvider.OrganizationURL = "https://my.test.site.gov";
                options.ServiceProvider.ContactPerson = new ContactType()
                {
                    Company = "Louisiana State Government - OTS",
                    GivenName = "Dina Heidar",
                    EmailAddress = new[] { "dina.heidar@la.gov" },
                    contactType = ContactTypeType.technical,
                    TelephoneNumber = new[] { "+1 234 5678" }
                };
                //Events - Modify events below if you want to log errors, add custom claims, etc.
                options.Events.OnRemoteFailure = context =>
                {
                    return Task.FromResult(0);
                };
                options.Events.OnTicketReceived = context =>
                {  //TODO: add custom claims here
                    return Task.FromResult(0);
                };
            })

                //.AddSamlCore("dev.adfs", options =>
                //{
                //    options.SignOutPath = "/signedout";
                //    options.ServiceProvider.EntityId = Configuration["AppConfiguration:ServiceProvider:EntityId"];
                //    options.MetadataAddress = Configuration["AppConfiguration:IdentityProvider:MetadataAddress"];
                //    options.ServiceProvider.CertificateIdentifierType = X509FindType.FindBySerialNumber;
                //    options.ServiceProvider.SigningCertificateX509TypeValue = "3BA5FEB170A86A9F42FEEB956C230D97";

            //    options.CreateMetadataFile = false;
            //    options.ForceAuthn = true;
            //    options.WantAssertionsSigned = true;
            //    options.RequireMessageSigned = false;

            //    // Service Provider Properties (optional) - These set the appropriate tags in the metadata.xml file
            //    //options.ServiceProvider.ApplicationProductionURL = "https://my.la.gov"; // this will create a production signin endpoint on the Idp side. This will be used when deployed to your production site
            //    //options.ServiceProvider.ApplicationStageURL = "https://dev.my.la.gov"; //this will create a stage signin endpoint on the Idp side. This will be used when deployed to your stage site
            //    //options.ServiceProvider.ServiceName = "My Test Site";
            //    //options.ServiceProvider.Language = "en-US";
            //    //options.ServiceProvider.OrganizationDisplayName = "Louisiana State Government";
            //    //options.ServiceProvider.OrganizationName = "Louisiana State Government";
            //    //options.ServiceProvider.OrganizationURL = "https://my.test.site.gov";
            //    //options.ServiceProvider.ContactPerson = new ContactType()
            //    //{
            //    //    Company = "Louisiana State Government - OTS",
            //    //    GivenName = "Dina Heidar",
            //    //    EmailAddress = new[] { "dina.heidar@la.gov" },
            //    //    contactType = ContactTypeType.technical,
            //    //    TelephoneNumber = new[] { "+1 234 5678" }
            //    //};

            //    //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //    //options.SignOutScheme = IdentityServerConstants.SignoutScheme;
            //    options.Events.OnRemoteFailure = context =>
            //    {
            //        context.Response.Redirect("/Home/AuthError");
            //        context.HandleResponse();
            //        return Task.CompletedTask;
            //    };
            //    options.Events.OnTicketReceived = context =>
            //    {
            //        return Task.CompletedTask;
            //    };

        //})
            .AddCookie();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication(); //add for Saml2Core

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}