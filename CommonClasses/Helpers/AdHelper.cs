using System;
using System.DirectoryServices;
using CommonClasses.MethodResults;

namespace CommonClasses.Helpers
{
    public class AdHelper
    {
        private string _server;

        public AdHelper(string server)
        {
            _server = server;
        }

        //public ADUserInfo GetUser(string userName)
        //{
        //    DirectoryEntry root = new DirectoryEntry("LDAP://" + _server);
        //    DirectorySearcher search = new DirectorySearcher(root);
        //    search.SearchScope = SearchScope.Subtree;
        //    search.Filter = "(sAMAccountName=" + userName.Substring(userName.IndexOf("\\") + 1) + ")";
        //    SearchResultCollection results = search.FindAll();
        //    return _GetUser(results[0].Path);
        //}

        //public ADUserInfo GetUserByFullName(string fullName)
        //{
        //    DirectoryEntry root = new DirectoryEntry("LDAP://" + _server);
        //    DirectorySearcher search = new DirectorySearcher(root);
        //    search.SearchScope = SearchScope.Subtree;
        //    search.Filter = "(displayName=" + fullName + ")";
        //    SearchResultCollection results = search.FindAll();
        //    return _GetUser(results[0].Path);
        //}

        //public List<ADUserInfo> GetMembers(string groupPath)
        //{
        //    DirectoryEntry root = new DirectoryEntry("LDAP://" + _server);
        //    DirectorySearcher search = new DirectorySearcher(root);
        //    List<ADUserInfo> members = new List<ADUserInfo>();
        //    search.SearchScope = SearchScope.Subtree;
        //    if (!String.IsNullOrEmpty(groupPath))
        //        search.Filter = "(memberOf=" + groupPath + ")";
        //    SearchResultCollection results = search.FindAll();
        //    foreach (SearchResult result in results)
        //    {
        //        members.Add(_GetUser(result.Path));
        //    }
        //    root.Close();

        //    return members;
        //}

        public BaseResult Authenticate(string userName, string password)
        {
            try
            {
                DirectoryEntry root = new DirectoryEntry("LDAP://" + _server, userName, password);

                DirectorySearcher search = new DirectorySearcher(root);
                search.SearchScope = SearchScope.Subtree;
                search.Filter = "(sAMAccountName=" + userName.Substring(userName.IndexOf("\\") + 1) + ")";
                SearchResultCollection results = search.FindAll();
                if (results.Count == 0)
                    return new BaseResult { ErrorMessage = Messages.WrongLoginOrPassword };
            }
            catch (Exception ex)
            {
                return new BaseResult{ErrorMessage = ex.Message};
            }

            return new BaseResult();
        }

        //private ADUserInfo _GetUser(string userPath)
        //{
        //    DirectoryEntry entry = new DirectoryEntry(userPath);
        //    ADUserInfo user = new ADUserInfo();

        //    user.UserName = (string)entry.Properties["sAMAccountName"].Value;
        //    user.FirstName = (string)entry.Properties["givenname"].Value;
        //    user.LastName = (string)entry.Properties["sn"].Value;
        //    user.Email = (string)entry.Properties["mail"].Value;

        //    foreach (string group in entry.Properties["memberOf"])
        //    {
        //        user.Groups.Add(_GetGroup(group));
        //    }

        //    return user;
        //}

        //private string _GetGroup(string path)
        //{
        //    string value = string.Empty;
        //    int index1 = path.IndexOf("=", 1);
        //    int index2 = path.IndexOf(",", 1);
        //    if (index1 != -1 && index2 != -1)
        //    {
        //        value = path.Substring((index1 + 1), (index2 - index1) - 1);
        //    }

        //    return value;
        //}
    }
}