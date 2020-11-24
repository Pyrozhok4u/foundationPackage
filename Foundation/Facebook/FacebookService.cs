using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Foundation.ConfigurationResolver;
using Foundation.Facebook.Editor;
using Foundation.Logger;
using Foundation.ServicesResolver;
using UnityEngine;
using UnityEngine.Events;
using Json = Facebook.MiniJSON.Json;

namespace Foundation.Facebook
{
    /// <summary>
    /// Manages the communication with the Facebook SDK
    /// </summary>
    public class FacebookService : BaseService
    {
        
        public class FacebookHideEvent : UnityEvent<bool> { }
        public class FacebookSDKInitializedEvent : UnityEvent<bool> { }

        public FacebookHideEvent OnUnityHiddenByFacebook = new FacebookHideEvent();
        public FacebookSDKInitializedEvent OnFacebookSDKInitialized = new FacebookSDKInitializedEvent();
        
        public bool Initialized => FB.IsInitialized;
        public AccessToken AccessToken => _cachedAccessToken;

        private AccessToken _cachedAccessToken;
        private string _cachedUsername;
        private Sprite _cachedProfilePicture;
        private List<object> _cachedFriendsList;

        #region Initialization
        
        /// <summary>
        /// Base Service implementation -called internally by service resolver
        /// </summary>
        protected override void Initialize()
        {
            FacebookConfig config = GetConfig<FacebookConfig>();
            if (!config.AutoInitialization) return;
            
            InitializeFacebookSDK();
        }
        
        // ReSharper disable once InconsistentNaming
        public void InitializeFacebookSDK()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(OnFacebookInitialized, OnFacebookSDKHideUnity);
            }
            else
            {
                this.LogError("InitializeFacebookSDK: SDK was already initialized!");
                OnFacebookInitialized();
            }
        }

        private void OnFacebookInitialized()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                OnFacebookSDKInitialized?.Invoke(true);
            }
            else
            {
                this.LogError("Couldn't initialize FacebookSDK, disabling manager instance");
                OnFacebookSDKInitialized?.Invoke(false);
            }
        }
        
        #endregion

        #region FacebookSDK Events

        private void OnFacebookSDKHideUnity(bool isGameShown)
        {
            //Time.timeScale = !isGameShown ? 0 : 1; 
            //TODO: replace with proper pause logic if needed here
            OnUnityHiddenByFacebook?.Invoke(isGameShown);
        }

        #endregion

        #region API Calls

        public void Login(Action<string> onTokenReceived, List<string> permissionsList)
        {
            FB.LogInWithReadPermissions(permissionsList, (loginResult) =>
            {
                if (loginResult.Error == null)
                {
                    _cachedAccessToken = AccessToken.CurrentAccessToken;
                    onTokenReceived?.Invoke(_cachedAccessToken.TokenString);
                }
            });
        }

        public void Logout()
        {
            FB.LogOut();
        }

        public void Share(string contentURL, string title, string description, string previewPhotoURL)
        {
            FB.ShareLink(new Uri(contentURL), title, description, new Uri(previewPhotoURL));
        }

        public void FacebookGameRequest(string title, string message)
        {
            FB.AppRequest(message, title: title);
        }

        #endregion

        #region Profile Picture

        public void GetUsername(Action<string> onUsernameReady, bool useCache = true)
        {
            // Use cached user name if exists
            if (useCache && _cachedUsername != null)
            {
                onUsernameReady?.Invoke(_cachedUsername);
                return;
            }
            
            // Fetch user name and cache it
            FB.API("me?fields=name", HttpMethod.GET, (result) =>
            {
                if (result.Error == null)
                {
                    IDictionary dict = Json.Deserialize(result.RawResult) as IDictionary;
                    _cachedUsername = dict["name"].ToString();
                    onUsernameReady?.Invoke(_cachedUsername);
                }
            });
        }

        #endregion

        #region Profile Picture

        public void GetProfilePicture(Action<Sprite> onProfilePictureReady, bool useCache = true)
        {
            // Use cached sprite if exists
            if (useCache && _cachedProfilePicture != null)
            {
                onProfilePictureReady?.Invoke(_cachedProfilePicture);
                return;
            }
            
            // Fetch user profile as sprite and cache it
            FB.API("/me/picture", HttpMethod.GET, (result) =>
            {
                if (result.Error == null)
                {
                    Rect rect = new Rect(0, 0, result.Texture.width, result.Texture.height);
                    _cachedProfilePicture = Sprite.Create(result.Texture, rect, new Vector2());
                    onProfilePictureReady?.Invoke(_cachedProfilePicture);
                }
            });
        }

        #endregion

        #region Friends List

        public void GetFriendsPlayingThisGame(Action<List<string>> onFriendsListReady, bool useCache = true)
        {
            // Use cached friends list if exists
            if (useCache && _cachedFriendsList != null)
            {
                onFriendsListReady?.Invoke(_cachedFriendsList.Select(dict =>
                        ((Dictionary<string, object>) dict)["name"].ToString()).ToList());
                return;
            }
            
            // Fetch friends list and cache it
            string query = "/me/friends";
            FB.API(query, HttpMethod.GET, result =>
            {
                Dictionary<string, object> dictionary = (Dictionary<string, object>) Json.Deserialize(result.RawResult);
                _cachedFriendsList = (List<object>) dictionary["data"];

                onFriendsListReady?.Invoke(_cachedFriendsList.Select(dict =>
                    ((Dictionary<string, object>) dict)["name"].ToString()).ToList());
            });
        }

        #endregion

        #region Base service implementation

        public override void Dispose()
        {
            _cachedProfilePicture = null;
            _cachedFriendsList?.Clear();
            _cachedFriendsList = null;
            
            OnUnityHiddenByFacebook?.RemoveAllListeners();
            OnFacebookSDKInitialized?.RemoveAllListeners();
        }

        #endregion
    }
}
