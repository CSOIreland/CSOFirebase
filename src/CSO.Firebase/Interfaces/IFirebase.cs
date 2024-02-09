namespace CSO.Firebase
{
    public interface IFirebase
    {
        bool Authenticate(string Uid, string AccessToken, dynamic ApiServicesHelper, dynamic LogObject);
        bool Logout(string Uid, string AccessToken, dynamic ApiServicesHelper, dynamic LogObject);
        IDictionary<string, dynamic> GetAllUsers(dynamic ApiServicesHelper, dynamic LogObject);
        bool DeleteUser(string uid, dynamic ApiServicesHelper, dynamic LogObject);
    }
}
