using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace WebApiSecurity
{
    public class OAuthServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            return Task.Run(() => 
                //password flow doesn't provide a client id so we validate all clients
                context.Validated()
            );
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return Task.Run(() =>
            {
                //here is where we check for user name and password from persisted storage 
                if (context.UserName != context.Password)
                {
                    context.Rejected();
                    return;
                }
                // create identity
                var id = new ClaimsIdentity("Embedded");
                //add any claims required (claims is embedded and can be extracted from the token)
                id.AddClaim(new Claim("sub", context.UserName)); //subject
                id.AddClaim(new Claim("role", "user")); //role
                context.Validated(id);
            });
        }
    }
}
