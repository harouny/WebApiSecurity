using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace WebApiSecurity.AuthorizationServer
{
    public class OAuthServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            return Task.Run(() => 
                context.Validated()
            );
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return Task.Run(() =>
            {
                if (context.UserName != context.Password)
                {
                    context.Rejected();
                    return;
                }
                // create identity
                var id = new ClaimsIdentity("Embedded");
                id.AddClaim(new Claim("sub", context.UserName));
                id.AddClaim(new Claim("role", "user"));
                context.Validated(id);
            });
        }
    }
}
