using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using DMVideoPlayer.Annotations;


namespace DMVideoPlayer
{
    public class DMPlayerController : INotifyPropertyChanged
    {

        //  private static string defaultUrl = "https://stage-01.dailymotion.com";
        private static string defaultUrl = "https://www.dailymotion.com";

        private static bool defaultIsTapEnabled = true;

        private static string HockeyAppId = "6d380067c4d848ce863b232a1c5f10ae";
        private static string version = "2.9.3";
        private static string bundleIdentifier = "com.dailymotion.dailymotion-alpha";
        //private static string bundleIdentifier = "WindowsSDK";
        private static string eventName = "dmevent";
        private static string eventNameV2 = "event=";
        private static string pathPrefix = "/embed/";
        private static string messageHandlerEvent = "triggerEvent";

        public event Action OnDmWebViewMessageUpdated;

        private string _baseUrl; // URL!
        public bool ApiReady { get; set; }
        public string VideoId { get; set; }
        public string AccessToken { get; set; }
        // public string loadedJsonData { get; set; }
        public IDictionary<string, string> WithParameters { get; set; }
        public bool IsHeroVideo { get; set; }
        public bool PendingPlay { get; set; }
        public bool ShowingAd { get; set; }
        public string BaseUrl
        {
            get { return _baseUrl ?? defaultUrl; }
            set { _baseUrl = value; }
        }

        private bool? _isTapEnabled; // URL!

        public bool IsTapEnabled
        {
            get { return _isTapEnabled ?? defaultIsTapEnabled; }
            set { _isTapEnabled = value; }
        }

        private WebView _dmVideoPlayer;

        public WebView DmVideoPlayer
        {
            get { return _dmVideoPlayer; }
            set
            {

                _dmVideoPlayer = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, string> _dmWebViewMessage;

        public KeyValuePair<string, string> DmWebViewMessage
        {
            get { return _dmWebViewMessage; }
            set
            {

                _dmWebViewMessage = value;
                OnDmWebViewMessageUpdated?.Invoke();
                OnPropertyChanged();
            }
        }

        /// Initialize a new instance of the player
        /// - Parameters:
        ///   - accessToken: An optional oauth token. If provided it will be passed as Bearer token to the player.
        ///   - withParameters:  The dictionary of configuration parameters that are passed to the player.
        ///   - withCookiesParameters:     An optional array of HTTPCookie values that are passed to the player.
        public void Init(string accessToken = "",
            IDictionary<string, string> withParameters = null,
            IDictionary<string, string> withCookiesParameters = null)
        {
            this.AccessToken = accessToken;

            //Creating a new webview when doing a new call
            if (DmVideoPlayer == null)
            {
                DmVideoPlayer = NewWebView();

                //setting cookies if needed
                if (withCookiesParameters != null)
                {
                    //if in params we have the keys v1st or tg then we need to send it to the player in a cookie
                    foreach (var cookie in withCookiesParameters)
                    {
                        //set cookie
                        SetCookieInWebView(cookie.Key, cookie.Value);
                    }
                }

                //Recieving the events the player is sending
                DmVideoPlayer.ScriptNotify += DmWebView_ScriptNotify;

                //creating http request message to send to the webview
                HttpRequestMessage request = NewRequest("", withParameters);

                //doing call
                DmVideoPlayer.NavigateWithHttpRequestMessage(request);
            }
        }

        public void Reset(string accessToken = "",
    IDictionary<string, string> withParameters = null,
    IDictionary<string, string> withCookiesParameters = null)
        {
            this.AccessToken = accessToken;

            //Creating a new webview when doing a new call
            {
                DmVideoPlayer = NewWebView();

                //setting cookies if needed
                if (withCookiesParameters != null)
                {
                    //if in params we have the keys v1st or tg then we need to send it to the player in a cookie
                    foreach (var cookie in withCookiesParameters)
                    {
                        //set cookie
                        SetCookieInWebView(cookie.Key, cookie.Value);
                    }
                }

                //Recieving the events the player is sending
                DmVideoPlayer.ScriptNotify += DmWebView_ScriptNotify;

                //creating http request message to send to the webview
                HttpRequestMessage request = NewRequest("", withParameters);

                //doing call
                DmVideoPlayer.NavigateWithHttpRequestMessage(request);
            }
        }

        /// Load a video with ID and optional OAuth token
        ///
        /// - Parameter videoId:        The video's XID
        ///   - accessToken: An optional oauth token. If provided it will be passed as Bearer token to the player.
        ///   - withParameters:  The dictionary of configuration parameters that are passed to the player.
        ///   - withCookiesParameters:     An optional array of HTTPCookie values that are passed to the player.
        public void Load(string videoId, string accessToken = "", IDictionary<string, string> withParameters = null, IDictionary<string, string> withCookiesParameters = null)
        {

            this.VideoId = videoId;
            this.WithParameters = withParameters;
            this.AccessToken = accessToken;

            //check base url
            //if (BaseUrl != null)
            {
                //Creating a new webview when doing a new call
                if (DmVideoPlayer == null)
                {
                    DmVideoPlayer = NewWebView();

                    //setting cookies if needed
                    if (withCookiesParameters != null)
                    {
                        //if in params we have the keys v1st or tg then we need to send it to the player in a cookie
                        foreach (var cookie in withCookiesParameters)
                        {
                            //set cookie
                            SetCookieInWebView(cookie.Key, cookie.Value);
                        }
                    }

                    //Recieving the events the player is sending
                    DmVideoPlayer.ScriptNotify += DmWebView_ScriptNotify;

                    //creating http request message to send to the webview
                    HttpRequestMessage request = NewRequest(videoId, withParameters);

                    //doing call
                    DmVideoPlayer.NavigateWithHttpRequestMessage(request);
                }
                else
                {
                    if (ApiReady)
                    {
                        Load();
                        PendingPlay = false;
                    }
                    else
                    {
                        PendingPlay = true;
                    }
                }
            }
        }

        public void Unload()
        {
            DmVideoPlayer.ScriptNotify -= DmWebView_ScriptNotify;
            DmVideoPlayer = null;
        }



        private void DmWebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var eventNames = getEventNames(e?.Value);

            if (eventNames != null)
            {
                foreach (var name in eventNames)
                {
                    //Debug.WriteLine(name.ToString());

                    switch (name.Value)
                    {
                        case "apiready":
                            {
                                ApiReady = true;
                                if (PendingPlay)
                                {
                                    PendingPlay = false;
                                    Load();
                                }

                                //Tracking.setPV5Info(map);
                                break;
                            }
                        case "ad_start":
                            {
                                ShowingAd = true;
                                break;
                            }
                        case "ad_end":
                            {
                                ShowingAd = false;
                                break;
                            }
                    }

                    DmWebViewMessage = name;
                }
            }
        }

        private Dictionary<string, string> getEventNames(string DirtyEvent)
        {
            var queryParameters = new Dictionary<string, string>();
            string[] querySegments = DirtyEvent.Split('&');
            foreach (string segment in querySegments)
            {
                string[] parts = segment.Split('=');
                if (parts.Length > 0)
                {
                    string key = WebUtility.UrlDecode(parts[0].Trim(new char[] { '?', ' ' }));
                    string val = WebUtility.UrlDecode(parts[1].Trim());

                    //making sure the key isnt present  
                    if (!queryParameters.ContainsKey(key))
                    {
                        queryParameters.Add(key, val);
                    }
                }
            }
            return queryParameters;
        }

        private void Load()
        {
            if (WithParameters == null)
                return;

            if (WithParameters.ContainsKey("jsonEnvironmentInfo"))
            {
                InitEnvironmentInfoVariables(WithParameters["jsonEnvironmentInfo"]);
            }


            if (WithParameters.ContainsKey("loadedJsonData"))
            {
                // Pause();
                CallPlayerMethod("load", VideoId, WithParameters["loadedJsonData"]);
            }
            else
            {
                //  Pause();
                CallPlayerMethod("load", VideoId);
            }

            if (WithParameters.ContainsKey("mute"))
            {
                if (WithParameters["mute"] == "true")
                {
                    Mute();
                }
                else
                {
                    Unmute();
                }
            }
        }

        /// Set a player property
        ///
        /// - Parameter jsonData: The data value to set
        public void InitEnvironmentInfoVariables(string jsonData)
        {
            CallPlayerMethod("setProp", "neon", jsonData);
        }

        private HttpRequestMessage NewRequest(string videoId, IDictionary<string, string> parameters = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, Url(videoId, parameters));

            if (!string.IsNullOrEmpty(this.AccessToken))
            {
                message.Headers.Add("Authorization", "Bearer " + this.AccessToken);
            }
            return message;
        }

        //Creating a new webview
        private WebView NewWebView()
        {
            var webView = new WebView(WebViewExecutionMode.SameThread);
            webView.IsTapEnabled = IsTapEnabled;
            webView.NavigationStarting += Wb_NavigationStarting;
            webView.Opacity = 1;
            return webView;
        }

        //this fixes user agent issues
        private void NavigateWithHeader(Uri uri)
        {
            var requestMsg = new Windows.Web.Http.HttpRequestMessage(HttpMethod.Get, uri);
            requestMsg.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063");
            DmVideoPlayer.NavigateWithHttpRequestMessage(requestMsg);

            DmVideoPlayer.NavigationStarting += Wb_NavigationStarting;
        }

        private void Wb_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            DmVideoPlayer.NavigationStarting -= Wb_NavigationStarting;
            args.Cancel = true;
            NavigateWithHeader(args.Uri);
        }

        private void SetCookieInWebView(string key, string value)
        {
            Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            Windows.Web.Http.HttpCookie cookie = new Windows.Web.Http.HttpCookie(key, ".dailymotion.com", "/");
            cookie.Value = value;
            filter.CookieManager.SetCookie(cookie, false);
        }


        private Uri Url(string videoId, IDictionary<string, string> parameters = null)
        {
            var components = String.Concat(BaseUrl, pathPrefix, videoId);

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            parameters["api"] = "nativeBridge";
            // parameters["objc_sdk_version"] = version;
            parameters["app"] = bundleIdentifier;
            //parameters["GK_PV5_ANTI_ADBLOCK"] = "0";
            parameters["GK_PV5_NEON"] = "1";

            var builder = new StringBuilder(components);
            if (parameters.Any())
                builder.Append("?");
            builder.Append(String.Join("&", from p in parameters select String.Format("{0}={1}", p.Key, p.Value)));

            return new Uri(builder.ToString());
        }

        public void ToggleControls(bool show)
        {
            var hasControls = show ? "1" : "0";
            NotifyPlayerApi(method: "controls", argument: hasControls);
        }

        private async void NotifyPlayerApi(string method, string argument = null)
        {
            string callingMethod = string.Format("player.api('{0}')", method);

            List<string> callingJsMethod = new List<string>();
            callingJsMethod.Add(callingMethod);

            try
            {
                await DmVideoPlayer?.InvokeScriptAsync("eval", callingJsMethod);
            }
            catch (Exception e)
            {
                // throw new Exception("Error : " + callingMethod);
            }
        }

        // private async void CallEvalWebviewMethod(string callMethod)
        private async void CallEvalWebviewMethod(string callMethod)
        {
            if (!callMethod.Contains("mute"))
            {
                Debug.WriteLine(callMethod);
            }

            List<string> callingJsMethod = new List<string>();
            callingJsMethod.Add(callMethod);

            try
            {
                await DmVideoPlayer?.InvokeScriptAsync("eval", callingJsMethod);
            }
            catch (Exception e)
            {
                //throw new Exception("Error : " + callMethod);
            }
        }

        public async void CallPlayerMethod(string method, string param, string dataJson = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("player.");
            builder.Append(method);
            builder.Append('(');
            builder.Append("'" + param + "'");

            if (dataJson != null)
            {
                builder.Append(",JSON.parse('" + dataJson + "')");
                // builder.Append("," + dataJson + "");
            }

            builder.Append(')');
            String js = builder.ToString();

            CallEvalWebviewMethod(js);
        }

        public void setHeroVideo(bool isHeroVideo)
        {
            IsHeroVideo = isHeroVideo;
        }

        public bool isHeroVideo()
        {
            return IsHeroVideo;
        }

        public void ToggleFullscreen()
        {
            NotifyPlayerApi("notifyFullscreenChanged");
        }

        public void Play()
        {
            Debug.Write("PLAYER", "play");
            NotifyPlayerApi("play");

            if (IsHeroVideo)
            {
                Mute();
            }
            else
            {
                Unmute();
            }
        }

        public void Pause()
        {
            //Debug.Write("PLAYER", "pause");
            NotifyPlayerApi("pause");
        }


        public void Mute()
        {
            //Debug.Write("PLAYER", "MUTE");
            NotifyPlayerApi("mute");
        }

        public void Unmute()
        {
            //Debug.Write("PLAYER", "unmute");
            NotifyPlayerApi("unmute");
        }


        public void Volume(double value)
        {
            if (value >= 0.0 && value <= 1.0)
            {
                //NotifyPlayerApi("setVolume", value.ToString());
                NotifyPlayerApi(string.Format("setVolume({0})", value.ToString()));
            }
        }



        public void Seek(int seconds)
        {
            //player.seek(30);
            CallEvalWebviewMethod(string.Format("player.seek({0})", seconds));
            //NotifyPlayerApi(method: "seek", argument: "\(to)");
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
