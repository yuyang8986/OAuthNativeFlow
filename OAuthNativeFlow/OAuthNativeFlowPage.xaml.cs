﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Auth;
using System.Net.Http;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;

namespace OAuthNativeFlow
{
    public partial class OAuthNativeFlowPage : ContentPage
    {
        private Account account;

        [Obsolete]
        private AccountStore store;

        [Obsolete]
        public OAuthNativeFlowPage()
        {
            InitializeComponent();

            store = AccountStore.Create();
        }

        [Obsolete]
        private void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            string clientId = null;
            string redirectUri = null;
            //Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientId = Constants.GoogleiOSClientId;
                    redirectUri = Constants.GoogleiOSRedirectUrl;
                    break;

                case Device.Android:
                    clientId = Constants.GoogleAndroidClientId;
                    redirectUri = Constants.GoogleAndroidRedirectUrl;
                    break;
            }

            account = store.FindAccountsForService(Constants.AppName).FirstOrDefault();

            var authenticator = new OAuth2Authenticator(
                clientId,
                null,
                Constants.GoogleScope,
                new Uri(Constants.GoogleAuthorizeUrl),
                new Uri(redirectUri),
                new Uri(Constants.GoogleAccessTokenUrl),
                null,
                true);

            authenticator.Completed += OnAuthCompleted;
            authenticator.Error += OnAuthError;

            AuthenticationState.Authenticator = authenticator;

            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(authenticator);
        }

        [Obsolete]
        private void OnFacebookLoginClicked(object sender, EventArgs e)
        {
            string clientId = null;
            string redirectUri = null;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientId = Constants.FacebookiOSClientId;
                    redirectUri = Constants.FacebookiOSRedirectUrl;
                    break;

                case Device.Android:
                    clientId = Constants.FacebookAndroidClientId;
                    redirectUri = Constants.FacebookAndroidRedirectUrl;
                    break;
            }

            account = store.FindAccountsForService(Constants.AppName).FirstOrDefault();

            var authenticator = new OAuth2Authenticator(
                clientId,
                Constants.FacebookScope,
                new Uri(Constants.FacebookAuthorizeUrl),
                new Uri(Constants.FacebookAccessTokenUrl),
                null);

            authenticator.Completed += OnAuthCompleted;
            authenticator.Error += OnAuthError;

            AuthenticationState.Authenticator = authenticator;

            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(authenticator);
        }

        [Obsolete]
        private async void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            if (e.IsAuthenticated)
            {
                if (authenticator.AuthorizeUrl.Host == "www.facebook.com")
                {
                    FacebookEmail facebookEmail = null;

                    var httpClient = new HttpClient();

                    var json = await httpClient.GetStringAsync($"https://graph.facebook.com/me?fields=id,name,first_name,last_name,email,picture.type(large)&access_token=" + e.Account.Properties["access_token"]);

                    facebookEmail = JsonConvert.DeserializeObject<FacebookEmail>(json);

                    await store.SaveAsync(account = e.Account, Constants.AppName);

                    Application.Current.Properties.Remove("Id");
                    Application.Current.Properties.Remove("FirstName");
                    Application.Current.Properties.Remove("LastName");
                    Application.Current.Properties.Remove("DisplayName");
                    Application.Current.Properties.Remove("EmailAddress");
                    Application.Current.Properties.Remove("ProfilePicture");

                    Application.Current.Properties.Add("Id", facebookEmail.Id);
                    Application.Current.Properties.Add("FirstName", facebookEmail.First_Name);
                    Application.Current.Properties.Add("LastName", facebookEmail.Last_Name);
                    Application.Current.Properties.Add("DisplayName", facebookEmail.Name);
                    Application.Current.Properties.Add("EmailAddress", facebookEmail.Email);
                    Application.Current.Properties.Add("ProfilePicture", facebookEmail.Picture.Data.Url);

                    await Navigation.PushAsync(new FilesPage());
                }
                else
                {
                    GoogleFiles googleFiles = null;
                    //GoogleAuthorizationCodeFlow.Initializer initializer = new GoogleAuthorizationCodeFlow.Initializer();
                    DriveService driveService = new DriveService();

                    //List<File> files = RetrieveAllFiles(driveService);
                    //Console.WriteLine(files.Count);
                    //// If the user is authenticated, request their basic user data from Google
                    //// UserInfoUrl = https://www.googleapis.com/oauth2/v2/userinfo
                    var request = new OAuth2Request("GET", new Uri(Constants.GooglDriveFilesUrl), null, e.Account);
                    var response = await request.GetResponseAsync();
                    if (response != null)
                    {
                        // Deserialize the data and store it in the account store
                        // The users email address will be used to identify data in SimpleDB
                        string data = await response.GetResponseTextAsync();
                        googleFiles = JsonConvert.DeserializeObject<GoogleFiles>(data);
                    }

                    //if (account != null)
                    //{
                    //    store.Delete(account, Constants.AppName);
                    //}

                    //await store.SaveAsync(account = e.Account, Constants.AppName);

                    //Application.Current.Properties.Remove("Id");
                    //Application.Current.Properties.Remove("FirstName");
                    //Application.Current.Properties.Remove("LastName");
                    //Application.Current.Properties.Remove("DisplayName");
                    //Application.Current.Properties.Remove("EmailAddress");
                    //Application.Current.Properties.Remove("ProfilePicture");

                    //Application.Current.Properties.Add("Id", user.Id);
                    //Application.Current.Properties.Add("FirstName", user.GivenName);
                    //Application.Current.Properties.Add("LastName", user.FamilyName);
                    //Application.Current.Properties.Add("DisplayName", user.Name);
                    //Application.Current.Properties.Add("EmailAddress", user.Email);
                    //Application.Current.Properties.Add("ProfilePicture", user.Picture);
                    var filePage = new FilesPage();
                    filePage.BindingContext = googleFiles.Items;
                    await Navigation.PushAsync(filePage);
                }
            }
        }

        public static List<File> RetrieveAllFiles(DriveService service)
        {
            List<File> result = new List<File>();
            FilesResource.ListRequest request = service.Files.List();

            do
            {
                try
                {
                    FileList files = request.Execute();

                    result.AddRange(files.Files);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));
            return result;
        }

        [Obsolete]
        private void OnAuthError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            Debug.WriteLine("Authentication error: " + e.Message);
        }
    }
}