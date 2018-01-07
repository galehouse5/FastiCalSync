using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;

namespace FastiCalSync.UI.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityUser
    {
        public string FullName { get; set; }
        public bool HasGoogleAccessToken { get; set; }
        public string GoogleAccessToken { get; set; }
        public string GoogleRefreshToken { get; set; }
        public string GoogleTokenType { get; set; }
        public DateTime? GoogleTokenExpirationUtc { get; set; }

        public void From(ExternalLoginInfo info)
        {
            Email = info.Principal.FindFirstValue(ClaimTypes.Email);
            UserName = info.Principal.FindFirstValue(ClaimTypes.Email);
            FullName = info.Principal.FindFirstValue(ClaimTypes.Name);
            HasGoogleAccessToken = true;
            GoogleAccessToken = info.AuthenticationTokens.Single(t => t.Name.Equals("access_token")).Value;
            GoogleRefreshToken = info.AuthenticationTokens.SingleOrDefault(t => t.Name.Equals("refresh_token"))?.Value ?? GoogleRefreshToken;
            GoogleTokenType = info.AuthenticationTokens.Single(t => t.Name.Equals("token_type")).Value;
            GoogleTokenExpirationUtc = DateTime.Parse(info.AuthenticationTokens.Single(t => t.Name.Equals("expires_at")).Value);
        }
    }
}
