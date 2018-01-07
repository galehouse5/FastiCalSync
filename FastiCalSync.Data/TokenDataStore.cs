using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using System;
using System.Threading.Tasks;

namespace FastiCalSync.Data
{
    public class TokenDataStore : IDataStore
    {
        private readonly Token token;
        private readonly TokenRepository repository;

        public TokenDataStore(Token token, TokenRepository repository)
        {
            this.token = token;
            this.repository = repository;
        }

        public Task ClearAsync()
            => throw new NotImplementedException();

        protected void AssertValidParameters<T>(string key)
        {
            if (!typeof(T).Equals(typeof(TokenResponse)))
                throw new NotSupportedException();

            if (!key.Equals("user"))
                throw new NotSupportedException();
        }

        public Task DeleteAsync<T>(string key)
            => throw new NotImplementedException();

        public async Task<T> GetAsync<T>(string key)
        {
            AssertValidParameters<T>(key);

            if (!token.HasGoogleAccessToken)
                return default(T);

            TokenResponse tokenValue = new TokenResponse
            {
                AccessToken = token.GoogleAccessToken,
                TokenType = token.GoogleTokenType,
                ExpiresInSeconds = 3600,
                RefreshToken = token.GoogleRefreshToken,
                IssuedUtc = token.GoogleTokenExpirationUtc - TimeSpan.FromSeconds(3600) ?? DateTime.MinValue
            };
            return await Task.FromResult((T)(object)tokenValue);
        }

        public async Task StoreAsync<T>(string key, T value)
        {
            AssertValidParameters<T>(key);

            TokenResponse tokenValue = (TokenResponse)(object)value;
            token.HasGoogleAccessToken = true;
            token.GoogleAccessToken = tokenValue.AccessToken;
            token.GoogleTokenType = tokenValue.TokenType;
            token.GoogleTokenExpirationUtc = tokenValue.IssuedUtc + TimeSpan.FromSeconds(tokenValue.ExpiresInSeconds ?? 0);
            token.GoogleRefreshToken = tokenValue.RefreshToken;
            await repository.Update(token);
        }
    }
}
