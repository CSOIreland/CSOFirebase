namespace CSO.Firebase
{
    public interface IFirebase
    {
        bool Authenticate(string Uid, string AccessToken, IDictionary<string, string> apiConfiguration, dynamic LogObject);
        bool Logout(string Uid, string AccessToken, IDictionary<string, string> apiConfiguration, dynamic LogObject);
        IDictionary<string, dynamic> GetAllUsers(IDictionary<string,string> apiConfiguration, dynamic LogObject);
        bool DeleteUser(string uid, IDictionary<string, string> apiConfiguration, dynamic LogObject);
    }
}
