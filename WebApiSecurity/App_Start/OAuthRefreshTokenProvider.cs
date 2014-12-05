using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace WebApiSecurity
{
    public class OAuthRefreshTokenProvider : IAuthenticationTokenProvider
    {
        //simple persistence logic
        //TODO: change persistence logic
        private static readonly ConcurrentDictionary<string, AuthenticationTicket> RefreshTokens = 
            new ConcurrentDictionary<string, AuthenticationTicket>();


        //Every time a token is requested, this method will be called to create a refresh token
        //Note: That is not a production ready code
        //TODO: Refresh token handles should be treated as secrets and should be stored hashed 
        //TODO: maybe only create a handle the first time, then re-use
        //TODO: consider storing only the hash of the handle
        public void Create(AuthenticationTokenCreateContext context)
        {
            var refreshTokenid = Guid.NewGuid().ToString();
            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = DateTime.UtcNow.AddDays(5) //long lived refresh token (5 days)
            };
            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);
            RefreshTokens.TryAdd(refreshTokenid, refreshTokenTicket);
            context.SetToken(refreshTokenid);
        }


        //Every time a request for token is called passing (refresh_token) as a grant_type this method will be called 
        //to associate the refresh token with the ticket already persisted
        public void Receive(AuthenticationTokenReceiveContext context)
        {
            AuthenticationTicket ticket;
            if (!RefreshTokens.TryRemove(context.Token, out ticket)) return;
            context.SetTicket(ticket);
        }

        public Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            return Task.Run(() => Create(context));
        }

        public Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            return Task.Run(() => Receive(context));
        }
    }
}