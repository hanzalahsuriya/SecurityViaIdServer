using System.IdentityModel.Tokens.Jwt;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Clear mapping ... so claim type are shows same as in Id_Token
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
 
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", opt =>
            {
                opt.SignInScheme = "Cookies";
                opt.Authority = "http://localhost:5000/";
                opt.ClientId = "imagegalleryclient";
                opt.ResponseType = "code id_token";
                opt.Scope.Clear();
                //opt.CallbackPath = new PathString();
                opt.Scope.Add("openid");
                opt.Scope.Add("profile");
                opt.Scope.Add("address");
                opt.SaveTokens = true;
                opt.ClientSecret = "secret";


                // By default this middleware filters lot of claims

                // amr claim is in the id_token but not showing because it was filtered down by this middleware
                opt.ClaimActions.Remove("amr");


                // there are claims that are comming in User.Claims but we want to filter it down to keep cookie small and 
                // if we need it it will come from the user info endpoint
                opt.ClaimActions.DeleteClaim("sid");
                opt.ClaimActions.DeleteClaim("idp");

                // we don't need to remvoe that as it
                //opt.ClaimActions.DeleteClaim("address"); 



                


                // getting claims from userinfo endpoint
                opt.GetClaimsFromUserInfoEndpoint = true;
                opt.SaveTokens = true;

                // shouldn't do that
                opt.RequireHttpsMetadata = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
