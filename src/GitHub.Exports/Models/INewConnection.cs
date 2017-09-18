using System;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Models
{
    public interface INewConnection
    {
        HostAddress HostAddress { get; }
        string UserName { get; }
        User User { get; }
        bool IsLoggedIn { get; }
        Exception ConnectionError { get; }
    }
}
