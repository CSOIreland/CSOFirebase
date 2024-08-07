﻿using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System.Dynamic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace CSO.Firebase
{
    public class Firebase : IFirebase
    {

        public Firebase()
        {
        }
        //For reference: https://firebase.google.com/docs/admin/setup

        /// <summary>
        /// Returns a firebase user based on Access Token
        // You must import System.Collections.Immutable Version 1.7.1 from NuGet
        /// </summary>
        /// <param name="ApiServicesHelper"></param>
        /// <param name="LogObject"></param> 
        /// <returns></returns>
        public bool Authenticate(string Uid, string AccessToken, IDictionary<string, string> apiConfiguration, dynamic LogObject)
        {
            if (!Convert.ToBoolean(apiConfiguration["API_FIREBASE_ENABLED"])) return false;

            try
            {
                if (Uid == null || AccessToken == null) return false;

                //Create the app if we need to
                if (FirebaseAuth.DefaultInstance == null)
                {
                    try
                    {
                        
                        byte[] byteArray = Encoding.UTF8.GetBytes(apiConfiguration["API_FIREBASE_CREDENTIAL"]);
                        MemoryStream fbStream = new MemoryStream(byteArray);
                        var defaultApp = FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromStream(fbStream)
                        });
                       
                    }
                    //FirebaseApp already exists - continue
                    catch (System.ArgumentException)
                    {
                        LogObject.Error("Firebase app exists already - attempt to continue");
                        var defaultApp = FirebaseApp.GetInstance(apiConfiguration["API_FIREBASE_APP_NAME"]);

                    }
                }

                //validate the token
                Task<FirebaseToken> tToken = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(AccessToken, true);

                tToken.Wait();

                if (tToken?.Result == null)
                    return false;
                var usr = FirebaseAuth.DefaultInstance.GetUserAsync(Uid);
                usr.Wait();

                return tToken.Result.Uid.Equals(Uid);


            }
            catch (Exception ex)
            {
                LogObject.Error("Error authenticating Firebase token: " + ex.Message + ": " + ex.GetType());
                return false;
            }
        }

        public bool Logout(string Uid, string AccessToken, IDictionary<string, string> apiConfiguration, dynamic LogObject)
        {
            if (!Convert.ToBoolean(apiConfiguration["API_FIREBASE_ENABLED"])) return false;
            try
            {
                if (Uid == null || AccessToken == null) return false;

                

                //Create the app if we need to
                if (FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance == null)
                {
                    try
                    {
                        
                            byte[] byteArray = Encoding.UTF8.GetBytes(apiConfiguration["API_FIREBASE_CREDENTIAL"]);
                            MemoryStream fbStream = new MemoryStream(byteArray);
                            var defaultApp = FirebaseApp.Create(new AppOptions()
                            {
                                Credential = GoogleCredential.FromStream(fbStream)
                            });
                        
                       
                    }
                    //FirebaseApp already exists - continue
                    catch (System.ArgumentException)
                    {
                        LogObject.Error("Firebase app exists already - attempt to continue");
                        var defaultApp = FirebaseApp.GetInstance(apiConfiguration["API_FIREBASE_APP_NAME"]);

                    }
                }
                var revoke = FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(Uid);
                revoke.Wait();


                //validate the token
                Task<FirebaseToken> tToken = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(AccessToken, true);

                tToken.Wait();
            }
            catch (FirebaseAuthException ex)
            {
                //The token has been revoked- logout successful
                if (ex.AuthErrorCode == AuthErrorCode.RevokedIdToken)
                {
                    return true;
                }
                else
                {
                    //something else happened
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    var a = ex.InnerException.GetType();
                    if (ex.InnerException.GetType().Name.Equals("FirebaseAuthException"))
                    {

                        return true;
                    }
                }
                LogObject.Error("Error authenticating Firebase token: " + ex.Message + ": " + ex.GetType());
                return false;
            }

            return true;
        }
        /// <summary>
        /// Get all of the Firebase users for this Firebase Project
        /// </summary>
        /// <param name="ApiServicesHelper"></param>
        /// <param name="LogObject"></param>
        /// <returns></returns>
        public IDictionary<string, dynamic> GetAllUsers(IDictionary<string,string> apiConfiguration, dynamic LogObject)
        {
           
            if (!Convert.ToBoolean(apiConfiguration["API_FIREBASE_ENABLED"])) return null;

            //We use hash as an identifer for the cache. This is based on the contents of the firebaseKey file
            //string hash;
            //using (StreamReader sr = new StreamReader(configPath))
            //{
            //    hash = Utility.GetSHA256(sr.ReadToEnd());
            //}

            Dictionary<string, dynamic> userList = new Dictionary<string, dynamic>();

            //Create the app if we need to

            if (FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance == null)
            {
                try
                {
                    
                    byte[] byteArray = Encoding.UTF8.GetBytes(apiConfiguration["API_FIREBASE_CREDENTIAL"]);
                    MemoryStream fbStream = new MemoryStream(byteArray);
                    var defaultApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromStream(fbStream)
                    });
                    
                }
                catch
                {
                    LogObject.Error("Firebase app exists already - attempt to continue");
                    var defaultApp = FirebaseApp.GetInstance(apiConfiguration["API_FIREBASE_APP_NAME"]);
                }
            }

            //Read from Firebase
            var pagedEnumerable = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.ListUsersAsync(new ListUsersOptions() { PageSize = 1000 });
            var responses = pagedEnumerable.AsRawResponses().GetAsyncEnumerator();
            bool allRead = false;
            while (!allRead)
            {
                var wt = responses.MoveNextAsync();
                if (!wt.Result) break;
                ExportedUserRecords response = responses.Current;
                foreach (ExportedUserRecord user in response.Users)
                {

                    dynamic obj = new ExpandoObject();
                    obj.Uid = user.Uid;
                    if (user.ProviderData.Length > 0)
                    {
                        obj.Email = user.ProviderData[0].Email ?? user.Email;
                        obj.DisplayName = user.ProviderData[0].DisplayName ?? user.DisplayName;
                    }
                    else
                    {
                        obj.Email = user.Email;
                        obj.DisplayName = user.DisplayName;
                    }
                    userList.Add(user.Uid, obj);
                }
            }


            return userList;
        }



        /// <summary>
        /// Delete a user from Firebase
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="ApiServicesHelper"></param>
        /// <param name="LogObject"></param>
        /// <returns></returns>
        public bool DeleteUser(string uid, IDictionary<string, string> apiConfiguration, dynamic LogObject)
        {
                if (!Convert.ToBoolean(apiConfiguration["API_FIREBASE_ENABLED"])) return false;
                try
            {
                if (FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance == null)
                {
                    try
                    {
                        
                            byte[] byteArray = Encoding.UTF8.GetBytes(apiConfiguration["API_FIREBASE_CREDENTIAL"]);
                            MemoryStream fbStream = new MemoryStream(byteArray);
                            var defaultApp = FirebaseApp.Create(new AppOptions()
                            {
                                Credential = GoogleCredential.FromStream(fbStream)
                            });
                        
                        
                    }
                    catch
                    {
                        LogObject.Error("Firebase app exists already - attempt to continue");
                        var defaultApp = FirebaseApp.GetInstance(apiConfiguration["API_FIREBASE_APP_NAME"]);
                    }
                }
                var token = FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
                token.Wait();
                LogObject.Debug("Firebase user deleted: " + uid);
                return true;
            }
            catch (Exception ex)
            {
                LogObject.Debug("Can't delete Firebase user: " + uid + " Error: " + ex.Message);
                return false;
            }
        }
    }


}
