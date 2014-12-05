using System;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(WebApiSecurity.Startup))]
namespace WebApiSecurity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            //use attribute based routes
            config.MapHttpAttributeRoutes();
            //token generation
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/token"), //endpoint where clients can request for token
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1), //time to expire the token
                Provider = new OAuthServerProvider(), //a custom provider to handle authentication
                RefreshTokenProvider = new OAuthRefreshTokenProvider(), //configure server to enable refresh tokens
                AllowInsecureHttp = true //enable token to be requested using http (should be removed in production)
            });
            //token consumption
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            //configure web api app
            app.UseWebApi(config);
           
        }
    }
}
