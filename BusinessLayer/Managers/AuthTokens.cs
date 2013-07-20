using System;
using System.Collections.Generic;
using CommonClasses.Helpers;
using CommonClasses.InfoClasses;

namespace BusinessLayer.Authentication
{
    public class AuthTokens
    {
        private readonly static AuthTokens _instance = new AuthTokens();
        public static AuthTokens Instance { get { return _instance; } }
        //TODO: put expiration to config
        public const int SESSION_EXPIRATION_IN_MINUTES = 20;

        private readonly Dictionary<string, AuthInfo> _dict = new Dictionary<string, AuthInfo>();
        private object _lock = new object();

        private string GetUniqueToken()
        {
            string result;
            do
            {
                result = RandomHelper.GetRandomString(10);
            } while (_dict.ContainsKey(result));
            
            return result;
        }

        public string AddAuth(AuthInfo authInfo)
        {
            string token = GetUniqueToken();
            authInfo.LastActiveDate = DateTime.UtcNow;
            authInfo.Token = token;
            _dict[token] = authInfo;
            return token;
        }

        public bool RemoveAuth(string token)
        {
            lock (_lock)
            {
                if(_dict.ContainsKey(token))
                    return _dict.Remove(token);
                return true; // already removed
            }
        }

        public AuthInfo GetAuth(string token)
        {
            lock (_lock)
            {
                AuthInfo result;
                _dict.TryGetValue(token, out result);
                //TODO: do we need remove token if authentication is already expired?
                if (IsAuthenticationExpired(result))
                    return null;
                result.LastActiveDate = DateTime.UtcNow;
                return result;
            }
        }

        public bool IsAuthenticationExpired(AuthInfo authInfo)
        {
            if(authInfo != null)
            {
                TimeSpan diff = DateTime.UtcNow.Subtract(authInfo.LastActiveDate);
                if (diff.Minutes < SESSION_EXPIRATION_IN_MINUTES)
                    return false;
            }
            return true;
        }

        public void RemoveAuthInfoForUser(int userId, int companyId)
        {
            var token = GetTokenByUserAndCompany(userId, companyId);
            if (!string.IsNullOrEmpty(token))
            {
                lock (_lock)
                {
                    _dict.Remove(token);
                }
            }
        }

        private string GetTokenByUserAndCompany(int userId, int companyId)
        {
            var token = string.Empty;
            foreach (var info in _dict)
            {
                if (info.Value.UserId == userId && info.Value.InstanceId == companyId)
                {
                    token = info.Key;
                    break;
                }
            }
            return token;
        }
    }
}