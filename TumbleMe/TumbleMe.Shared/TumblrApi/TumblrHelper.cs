using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TumbleMe.Common;
using TumbleMe.TumblrApi;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace TumbleMe.TumblrApi
{
    public class TumblrHelper
    {
        const string TumblrUserAccessToken = "TumblrUserAccessToken";
        const string TumblrUserSecret = "TumblrUserSecret";
        const string TumblrUsername = "TumblrUsername";

        const string RequestTokenUrl = "https://www.tumblr.com/oauth/request_token";
        const string AuthorizeUrl = "https://www.tumblr.com/oauth/authorize";
        const string AccessTokenUrl = "https://www.tumblr.com/oauth/access_token";
        const string UserInfoUrl = "https://api.tumblr.com/v2/user/info";
        const string PhotoPostUrl = "http://api.tumblr.com/v2/blog/build2014.tumblr.com/posts/photo";
        const string PostPhotoToBlogUrl = "https://api.tumblr.com/v2/blog/build2014.tumblr.com/post";

        #region AppKey and AppSecret

        //Please set up valid AppKey and AppSecret to get started
        const string AppKey = "FILL THIS IN";
        const string AppSecret = "FILL THIS IN";

        #endregion

        ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

        /// <summary>
        /// Gets or sets a flag indicating whether a user is signed in and 
        /// authorized to post
        /// </summary>
        public bool SignedIn { get; private set; }

        /// <summary>
        /// Gets or sets the username for an authenticated user who has
        /// access to the blog
        /// </summary>
        public string CurrentUsername
        {
            get;
            private set;
        }

        //Access token for current authenticated/authorized user
        private string userAccessToken = null;

        //Oauth secret token for current authenticated/authorized user
        //This is used to sign requests that require user authentication
        //such as post requests
        private string userSecret = null;

        public TumblrHelper()
        {
            if (roamingSettings.Values.ContainsKey(TumblrUsername) &&
                roamingSettings.Values.ContainsKey(TumblrUserAccessToken) &&
                roamingSettings.Values.ContainsKey(TumblrUserSecret))
            {
                //Pick up the username, user access token and user secret from the roaming settings
                CurrentUsername = roamingSettings.Values[TumblrUsername] as string;
                userAccessToken = roamingSettings.Values[TumblrUserAccessToken] as string;
                userSecret = roamingSettings.Values[TumblrUserSecret] as string;

                SignedIn = true;
            }
        }

        public void SignOut()
        {
            if (SignedIn)
            {
                CurrentUsername = null;
                if (roamingSettings.Values.ContainsKey(TumblrUsername))
                {
                    roamingSettings.Values.Remove(TumblrUsername);
                }

                userAccessToken = null;
                if (roamingSettings.Values.ContainsKey(TumblrUserAccessToken))
                {
                    roamingSettings.Values.Remove(TumblrUserAccessToken);
                }

                userSecret = null;
                if (roamingSettings.Values.ContainsKey(TumblrUserSecret))
                {
                    roamingSettings.Values.Remove(TumblrUserSecret);
                }
            }
        }

#if WINDOWS_APP
        public async Task<AuthenticationResult> StartAuthentication()
        {
            string oauth_token_secret = null;
            string oauth_token = null;
            string[] retVals = await GetTumblrRequestTokenAsync();
            oauth_token = retVals[0];
            oauth_token_secret = retVals[1];           
            
            string tumblrUrl = AuthorizeUrl + "?oauth_token=" + oauth_token;
            Uri startUri = new Uri(tumblrUrl, UriKind.Absolute);
            Uri endUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

            var authentication = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None,
                startUri,
                endUri);
            return await ContinueAuthentication(authentication, oauth_token_secret);
        }
#endif

#if WINDOWS_PHONE_APP
        public async Task StartAuthentication()
        {
            string oauth_token_secret = null;
            string oauth_token = null;
            string[] retVals = await GetTumblrRequestTokenAsync();
            oauth_token = retVals[0];
            oauth_token_secret = retVals[1];           
            
            string tumblrUrl = AuthorizeUrl + "?oauth_token=" + oauth_token;
            Uri startUri = new Uri(tumblrUrl, UriKind.Absolute);
            Uri endUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

            var continuationData = new ValueSet();
            continuationData["oauth_token_secret"] = oauth_token_secret;
            WebAuthenticationBroker.AuthenticateAndContinue(
                startUri,
                endUri,
                continuationData,
                WebAuthenticationOptions.None);            
        }

        public async Task<AuthenticationResult> ContinueAuthentication(WebAuthenticationBrokerContinuationEventArgs continueWab)
        {
            string oauth_token_secret = continueWab.ContinuationData["oauth_token_secret"] as string;

            return await ContinueAuthentication(continueWab.WebAuthenticationResult, oauth_token_secret);
        }
#else
        public async Task<AuthenticationResult> AuthenticateUser()
        {
            string oauth_token_secret = null;
            string oauth_token = null;
            string[] retVals = await GetTumblrRequestTokenAsync();
            oauth_token = retVals[0];
            oauth_token_secret = retVals[1];

            string tumblrUrl = AuthorizeUrl + "?oauth_token=" + oauth_token;
            Uri startUri = new Uri(tumblrUrl, UriKind.Absolute);
            Uri endUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

            WebAuthenticationResult webAuthResult = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None,
                startUri,
                endUri);

            return await ContinueAuthentication(webAuthResult, oauth_token_secret);
        }
#endif

        public async Task PostToBlog(StorageFile fileToPost, string caption)
        {
            string tumblrUrl = PostPhotoToBlogUrl;

            byte[] data = null;
            using (IRandomAccessStreamWithContentType stream = await fileToPost.OpenReadAsync())
            {
                using (var reader = new DataReader(stream))
                {
                    data = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(data);
                }
            }

            if (data == null)
                return;

            string timeStamp = GetTimeStamp();
            string nonce = GetNonce();

            String SigBaseStringParams = await Task.Run(() =>
            {
                StringBuilder str = new StringBuilder();
                str.Append("caption=" + EscapeOAuthString(caption));
                str.Append("&" + "data=" + UrlEncode(data, false, false));
                str.Append("&" + "oauth_consumer_key=" + AppKey);
                str.Append("&" + "oauth_nonce=" + nonce);
                str.Append("&" + "oauth_signature_method=HMAC-SHA1");
                str.Append("&" + "oauth_timestamp=" + timeStamp);
                str.Append("&" + "oauth_token=" + userAccessToken);
                str.Append("&" + "oauth_version=1.0");
                str.Append("&" + "type=photo");
                return str.ToString();
            });
            String SigBaseString = "POST&";
            SigBaseString += await Task.Run(() =>
            {
                return Uri.EscapeDataString(PostPhotoToBlogUrl) + "&" + EscapeBigString(SigBaseStringParams);
            });

            String Signature = GetSignature(SigBaseString, AppSecret, userSecret);

            var encData = await Task.Run(() => { return UrlEncode(data, true, true); });
            HttpStringContent httpContent = await Task.Run(() =>
            {
                return new HttpStringContent("type=photo&caption=" + Uri.EscapeDataString(caption) + "&data=" + encData);
            });
            httpContent.Headers.ContentType = HttpMediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            string authorizationHeaderParams =
                "oauth_consumer_key=\"" + AppKey +
                "\", oauth_nonce=\"" + nonce +
                "\", oauth_signature_method=\"HMAC-SHA1" +
                "\", oauth_signature=\"" + Uri.EscapeDataString(Signature) +
                "\", oauth_timestamp=\"" + timeStamp +
                "\", oauth_token=\"" + Uri.EscapeDataString(userAccessToken) +
                "\", oauth_version=\"1.0\"";

            var httpClient = new Windows.Web.Http.HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("OAuth", authorizationHeaderParams);
            var httpResponseMessage = await httpClient.PostAsync(new Uri(tumblrUrl), httpContent);
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public async Task<List<Post>> GetPhotoPosts(int offset, int pageSize)
        {
            string tumblrUrl = PhotoPostUrl + "?api_key=" + AppKey + "&offset=" + offset + "&limit=" + pageSize + "&filter=text&timestamp=" + GetTimeStamp();
            var client = new Windows.Web.Http.HttpClient();
            string response = await client.GetStringAsync(new Uri(tumblrUrl, UriKind.Absolute));

            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(TumblrApi.RootObject));
            var root = jsonDeserializer.ReadObject(new MemoryStream(Encoding.Unicode.GetBytes(response))) as TumblrApi.RootObject;

            return root.response.posts;
        }

        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";       
        private const string Digit = "1234567890";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string AlphaNumeric = Upper + Lower + Digit;
        private const string Unreserved = AlphaNumeric + "-._~";

        private string UrlEncode(byte[] value, bool encodePercent, bool encodeTilde)
        {
            var osb = new StringBuilder(value.Length * 3);
            for (var x = 0; x < value.Length; x++)
            {
                var b = value[x];
                if (!Unreserved.Contains((char)b) && ((char)b) != '~' && (((char)b) != '%' || encodePercent || !encodeTilde))
                {
                    osb.AppendFormat("%{0:X2}", b);
                }
                else
                {
                    osb.Append((char)b);
                }
            }

            if (encodeTilde)
            {
                osb = osb.Replace("~", "%7E");
            }
            return osb.Replace("%%", "%25%").ToString(); // Revisit to encode actual %'s
        }

        private string EscapeOAuthString(string text)
        {
            string value = text;

            value = Uri.EscapeDataString(value).Replace("+", "%20");

            // UrlEncode escapes with lowercase characters (e.g. %2f) but oAuth needs %2F
            value = Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());

            // these characters are not escaped by UrlEncode() but needed to be escaped
            value = value.Replace("(", "%28").Replace(")", "%29").Replace("$", "%24").Replace("!", "%21").Replace(
                "*", "%2A").Replace("'", "%27");

            // these characters are escaped by UrlEncode() but will fail if unescaped!
            value = value.Replace("%7E", "~");

            return value;
        }

        private string EscapeBigString(string text)
        {
            byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            string answer = UrlEncode(bytes, false, false);
            return answer;
        }

        private async Task<AuthenticationResult> ContinueAuthentication(WebAuthenticationResult webAuthResult, string oauth_token_secret)
        {
            AuthenticationResult authResult = new AuthenticationResult();

            if (webAuthResult.ResponseStatus != WebAuthenticationStatus.Success)
            {
                return authResult;
            }

            //Did the user say no?
            int oauth_token_index = webAuthResult.ResponseData.IndexOf("oauth_token");
            if(oauth_token_index < 0)
            {
                //Yup, looks like the user did not authorize. We're done
                return authResult;
            }

            string responseData = webAuthResult.ResponseData.Substring(oauth_token_index);
            string request_token = null;
            string oauth_verifier = null;
            String[] keyValPairs = responseData.Split('&');

            for (int i = 0; i < keyValPairs.Length; i++)
            {
                String[] splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        request_token = keyValPairs[i].Substring("oauth_token=".Length);
                        break;
                    case "oauth_verifier":
                        oauth_verifier = keyValPairs[i].Substring("oauth_verifier=".Length);
                        break;
                }
            }

            //Tumblr sticks some gunk at the end of the oauth verifier (can't figure out why)
            //We need to remove it if we want our signature to look right
            if (oauth_verifier.EndsWith("#_=_"))
            {
                oauth_verifier = oauth_verifier.Remove(oauth_verifier.Length - 4);
            }

            string timeStamp = GetTimeStamp();
            string nonce = GetNonce();

            String SigBaseStringParams = "oauth_consumer_key=" + AppKey;
            SigBaseStringParams += "&" + "oauth_nonce=" + nonce;
            SigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
            SigBaseStringParams += "&" + "oauth_timestamp=" + timeStamp;
            SigBaseStringParams += "&" + "oauth_token=" + request_token;
            SigBaseStringParams += "&" + "oauth_verifier=" + oauth_verifier;
            SigBaseStringParams += "&" + "oauth_version=1.0";
            String SigBaseString = "POST&";
            SigBaseString += Uri.EscapeDataString(AccessTokenUrl) + "&" + Uri.EscapeDataString(SigBaseStringParams);

            String Signature = GetSignature(SigBaseString, AppSecret, oauth_token_secret);

            HttpStringContent httpContent = new HttpStringContent("oauth_verifier=" + Uri.EscapeDataString(oauth_verifier), Windows.Storage.Streams.UnicodeEncoding.Utf8);
            httpContent.Headers.ContentType = HttpMediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            string authorizationHeaderParams =
                "oauth_consumer_key=\"" + AppKey +
                "\", oauth_nonce=\"" + nonce +
                "\", oauth_signature_method=\"HMAC-SHA1" +
                "\", oauth_signature=\"" + Uri.EscapeDataString(Signature) +
                "\", oauth_timestamp=\"" + timeStamp +
                "\", oauth_token=\"" + Uri.EscapeDataString(request_token) +
                "\", oauth_version=\"1.0\"";

            var httpClient = new Windows.Web.Http.HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("OAuth", authorizationHeaderParams);
            var httpResponseMessage = await httpClient.PostAsync(new Uri(AccessTokenUrl), httpContent);
            string response = await httpResponseMessage.Content.ReadAsStringAsync();

            String[] Tokens = response.Split('&');

            for (int i = 0; i < Tokens.Length; i++)
            {
                String[] splits = Tokens[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        userAccessToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        userSecret = splits[1];
                        break;
                }
            }

            if (string.IsNullOrEmpty(userAccessToken) || string.IsNullOrEmpty(userSecret))
            {
                throw new Exception("Could not authenticate!");
            }

            //Get user info

            timeStamp = GetTimeStamp();
            nonce = GetNonce();

            SigBaseStringParams = "oauth_consumer_key=" + AppKey;
            SigBaseStringParams += "&" + "oauth_nonce=" + nonce;
            SigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
            SigBaseStringParams += "&" + "oauth_timestamp=" + timeStamp;
            SigBaseStringParams += "&" + "oauth_token=" + userAccessToken;
            SigBaseStringParams += "&" + "oauth_version=1.0";
            SigBaseString = "GET&";
            SigBaseString += Uri.EscapeDataString(UserInfoUrl) + "&" + Uri.EscapeDataString(SigBaseStringParams);

            Signature = GetSignature(SigBaseString, AppSecret, userSecret);

            authorizationHeaderParams =
                "oauth_consumer_key=\"" + AppKey +
                "\", oauth_nonce=\"" + nonce +
                "\", oauth_signature_method=\"HMAC-SHA1" +
                "\", oauth_signature=\"" + Uri.EscapeDataString(Signature) +
                "\", oauth_timestamp=\"" + timeStamp +
                "\", oauth_token=\"" + Uri.EscapeDataString(userAccessToken) +
                "\", oauth_version=\"1.0\"";

            httpClient = new Windows.Web.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("OAuth", authorizationHeaderParams);
            response = await httpClient.GetStringAsync(new Uri(UserInfoUrl));

            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(TumblrApi.RootObject));
            var root = jsonDeserializer.ReadObject(new MemoryStream(Encoding.Unicode.GetBytes(response))) as TumblrApi.RootObject;

            bool userHasAccess = (from blog in root.response.user.blogs
                                  where blog.name == "build2014"
                                  select blog).Count() > 0;

            authResult.UserAuthenticated = true;
            authResult.User = root.response.user;

            if (userHasAccess)
            {
                CurrentUsername = root.response.user.name;
                SignedIn = true;

                roamingSettings.Values[TumblrUserAccessToken] = userAccessToken;
                roamingSettings.Values[TumblrUserSecret] = userSecret;
                roamingSettings.Values[TumblrUsername] = CurrentUsername;

                authResult.UserAuthorized = true;
            }

            return authResult;
        }

        private async Task<string[]> GetTumblrRequestTokenAsync()
        {
            string callbackUrl = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
            string nonce = GetNonce();
            string timeStamp = GetTimeStamp();
            string SigBaseStringParams = "oauth_callback=" + Uri.EscapeDataString(callbackUrl);
            SigBaseStringParams += "&" + "oauth_consumer_key=" + AppKey;
            SigBaseStringParams += "&" + "oauth_nonce=" + nonce;
            SigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
            SigBaseStringParams += "&" + "oauth_timestamp=" + timeStamp;
            SigBaseStringParams += "&" + "oauth_version=1.0";
            string SigBaseString = "GET&";
            SigBaseString += Uri.EscapeDataString(RequestTokenUrl) + "&" + Uri.EscapeDataString(SigBaseStringParams);
            string Signature = GetSignature(SigBaseString, AppSecret, null);

            string requestUrl = RequestTokenUrl + "?" + SigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(Signature);
            var httpClient = new Windows.Web.Http.HttpClient();
            string GetResponse = await httpClient.GetStringAsync(new Uri(requestUrl));


            string request_token = null;
            string oauth_token_secret = null;
            string[] keyValPairs = GetResponse.Split('&');

            for (int i = 0; i < keyValPairs.Length; i++)
            {
                string[] splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        request_token = keyValPairs[i].Substring("oauth_token=".Length);
                        break;

                    case "oauth_token_secret":
                        oauth_token_secret = keyValPairs[i].Substring("oauth_token_secret=".Length);
                        break;
                }
            }

            string[] retVals = new string[2];
            retVals[0] = request_token;
            retVals[1] = oauth_token_secret;
            return retVals;
        }

        string GetNonce()
        {
            Random rand = new Random();
            int nonce = rand.Next(1000000000);
            return nonce.ToString();
        }

        string GetTimeStamp()
        {            
            TimeSpan SinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Round(SinceEpoch.TotalSeconds).ToString();
        }

        string GetSignature(string sigBaseString, string appOauthSecret, string userOauthSecret)
        {
            string key = Uri.EscapeDataString(appOauthSecret);
            if (string.IsNullOrEmpty(userOauthSecret))
            {
                key += "&";
            }
            else
            {
                key += "&" + Uri.EscapeDataString(userOauthSecret);
            }

            IBuffer KeyMaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            CryptographicKey MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
            IBuffer DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(sigBaseString, BinaryStringEncoding.Utf8);
            IBuffer SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
            string Signature = CryptographicBuffer.EncodeToBase64String(SignatureBuffer);

            return Signature;
        }
    }
}
