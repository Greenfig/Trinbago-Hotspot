//using DotNetOpenAuth.AspNet.Clients;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.IO;
//using System.Net;
//using System.Web;
//namespace Trinbago_MVC5.Models
//{
//    /// <summary>
//    /// A DotNetOpenAuth client for logging in to Twitter using OAuth2.
//    /// Reference: http://developers.facebook.com/docs/howtos/login/server-side-login/
//    /// </summary>
//    public class TwitterOAuth2Client : OAuth2Client
//    {

//        #region Constants and Fields

//        /// <summary>
//        /// The authorization endpoint.
//        /// </summary>
//        private const string AuthorizationEndpoint = "https://www.facebook.com/v2.8/dialog/oauth";

//        /// <summary>
//        /// The token endpoint.
//        /// </summary>
//        private const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";

//        /// <summary>
//        /// The user info endpoint.
//        /// </summary>
//        private const string UserInfoEndpoint = "https://graph.facebook.com/me";

//        /// <summary>
//        /// The app id.
//        /// </summary>
//        private readonly string _appId;

//        /// <summary>
//        /// The app secret.
//        /// </summary>
//        private readonly string _appSecret;

//        /// <summary>
//        /// The requested scopes.
//        /// </summary>
//        private readonly string[] _requestedScopes;

//        #endregion

//        /// <summary>
//        /// Creates a new Twitter OAuth2 client, requesting the default "email" scope.
//        /// </summary>
//        public TwitterOAuth2Client(string appId, string appSecret)
//            : this(appId, appSecret, new[] { "email" }) { }

//        /// <summary>
//        /// Creates a new Twitter OAuth2 client.
//        /// </summary>
//        public TwitterOAuth2Client(string appId, string appSecret, params string[] requestedScopes)
//            : base("facebook")
//        {
//            if (string.IsNullOrWhiteSpace(appId))
//                throw new ArgumentNullException("appId");

//            if (string.IsNullOrWhiteSpace(appSecret))
//                throw new ArgumentNullException("appSecret");

//            if (requestedScopes == null)
//                throw new ArgumentNullException("requestedScopes");

//            if (requestedScopes.Length == 0)
//                throw new ArgumentException("One or more scopes must be requested.", "requestedScopes");

//            _appId = appId;
//            _appSecret = appSecret;
//            _requestedScopes = requestedScopes;
//        }

//        protected override Uri GetServiceLoginUrl(Uri returnUrl)
//        {
//            var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);

//            return BuildUri(AuthorizationEndpoint, new NameValueCollection
//                {
//                    { "client_id", _appId },                    
//                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
//                    { "state", state },                    
//                    { "auth_type", "rerequest"},
//                    { "scope", string.Join(" ", _requestedScopes) }
//                });
//        }

//        protected override IDictionary<string, string> GetUserData(string accessToken)
//        {
//            var scope = String.Join(",", _requestedScopes);
//            var uri = BuildUri(UserInfoEndpoint, new NameValueCollection { { "fields", "id,name" + (scope != null ? ","+scope : null)}, { "access_token", accessToken } });

//            var webRequest = (HttpWebRequest) WebRequest.Create(uri);

//            using (var webResponse = webRequest.GetResponse())
//            using (var stream = webResponse.GetResponseStream())
//            {
//                if (stream == null)
//                    return null;

//                using (var textReader = new StreamReader(stream))
//                {
//                    var json = textReader.ReadToEnd();
//                    var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
//                    var data = extraData.ToDictionary(x => x.Key, x => x.Value.ToString());

//                    data.Add("picture", string.Format("https://graph.facebook.com/{0}/picture", data["id"]));

//                    return data;
//                }
//            }
//        }

//        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
//        {
//            var uri = BuildUri(TokenEndpoint, new NameValueCollection
//                {
//                    { "code", authorizationCode },
//                    { "client_id", _appId },
//                    { "client_secret", _appSecret },
//                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
//                });

//            var webRequest = (HttpWebRequest) WebRequest.Create(uri);

//            using (var webResponse = webRequest.GetResponse())
//            {
//                var responseStream = webResponse.GetResponseStream();
//                if (responseStream == null)
//                    return null;

//                using (var reader = new StreamReader(responseStream))
//                {
//                    var response = reader.ReadToEnd();

//                    var results = HttpUtility.ParseQueryString(response);
//                    return results["access_token"];
//                }
//            }
//        }

//        private static Uri BuildUri(string baseUri, NameValueCollection queryParameters)
//        {
//            var keyValuePairs = queryParameters.AllKeys.Select(k => HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(queryParameters[k]));
//            var qs = String.Join("&", keyValuePairs);

//            var builder = new UriBuilder(baseUri) { Query = qs };
//            return builder.Uri;
//        }
//    }
//}