using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace WebApiSecurity
{
    public class OAuthServerProvider : OAuthAuthorizationServerProvider
    {
        //validate client credentials (called when requesting a token by user/password and also when renewing token using a refresh token)
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            return Task.Run(() =>
            {
                string clientId, clientSecret;
                //expects client_id, client_secret to be passed in request body
                context.TryGetFormCredentials(out clientId, out clientSecret);
                if (clientId == clientSecret && !string.IsNullOrWhiteSpace(clientId)) //TODO: Replace Demo Only Check
                {
                    //need to make the client_id available for later security checks
                    context.OwinContext.Set("as:client_id", clientId);
                    context.Validated();
                    return;
                }
                context.Rejected();
            });
        }

        //called when a request for token is called passing (password) as a grant_type (Resource Owner Flow)
        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return Task.Run(() =>
            {
                //here is where we check for user name and password from persisted storage 
                if (context.UserName != context.Password && !string.IsNullOrWhiteSpace(context.UserName)) //TODO: Replace Demo Only Check
                {
                    context.Rejected();
                    return;
                }
                // create identity
                var id = new ClaimsIdentity("Embedded");
                //add any claims required (claims is embedded and can be extracted from the token)
                id.AddClaim(new Claim("sub", context.UserName)); //subject
                id.AddClaim(new Claim("role", "user")); //role
                var properties = new Dictionary<string, string>();
                // we need to add the client id to the properties of the AuthenticationTicket, later the ticket will be passed to the refresh token provider
                if (!string.IsNullOrWhiteSpace(context.ClientId))
                {
                    properties.Add("as:client_id", context.ClientId);
                }
                var ticket = new AuthenticationTicket(id, new AuthenticationProperties(properties));
                context.Validated(ticket);
            });
        }

        //called when a request for token is called passing (refresh_token) as a grant_type
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            return Task.Run(() =>
            {
                var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
                var currentClient = context.OwinContext.Get<string>("as:client_id");
                // enforce client binding of refresh token
                if (originalClient != currentClient)
                {
                    context.Rejected();
                    return;
                }
                // chance to change authentication ticket for refresh token requests
                var newId = new ClaimsIdentity(context.Ticket.Identity);
                var newTicket = new AuthenticationTicket(newId, context.Ticket.Properties);
                context.Validated(newTicket);
            });
        }
    }
}
