using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Api
{
    public interface INewConnectionManager
    {
        ObservableCollection<INewConnection> Connections { get; }

        Task<INewConnection> GetConnection(HostAddress address);
        Task<ObservableCollection<INewConnection>> GetLoadedConnections();
        Task<INewConnection> LogIn(HostAddress address, string username, string password);
        Task LogOut(HostAddress address);
    }
}