using System;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public class NewConnection : INewConnection
    {
        public NewConnection(
            HostAddress hostAddress,
            string userName,
            User user,
            Exception connectionError)
        {
            HostAddress = hostAddress;
            UserName = userName;
            User = user;
            ConnectionError = connectionError;
        }

        public HostAddress HostAddress { get; }
        public string UserName { get; }
        public User User { get; }
        public bool IsLoggedIn => ConnectionError == null;
        public Exception ConnectionError { get; }
    }
}
