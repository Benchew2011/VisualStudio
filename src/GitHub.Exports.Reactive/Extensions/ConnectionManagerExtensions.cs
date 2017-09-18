using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.Extensions
{
    public static class ConnectionManagerExtensions
    {
        public static async Task<bool> IsLoggedIn(this INewConnectionManager cm, IRepositoryHosts hosts)
        {
            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.ConnectionError == null);
        }

        public static async Task<bool> IsLoggedIn(this INewConnectionManager cm, IRepositoryHosts hosts, HostAddress address)
        {
            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.HostAddress == address && x.ConnectionError == null);
        }

        public static async Task<bool> IsLoggedIn(this INewConnectionManager cm, HostAddress address)
        {
            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.HostAddress == address && x.ConnectionError == null);
        }

        public static async Task<INewConnection> LookupConnection(this INewConnectionManager cm, IRepositoryModel repository)
        {
            if (repository != null)
            {
                var address = HostAddress.Create(repository.CloneUrl);
                var connections = await cm.GetLoadedConnections();
                return connections.FirstOrDefault(x => x.HostAddress == address);
            }

            return null;
        }

        public static IObservable<bool> IsLoggedIn(this IConnectionManager cm, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            return cm.Connections.ToObservable()
                    .SelectMany(c => c.Login())
                    .Any(c => hosts.LookupHost(c.HostAddress).IsLoggedIn);
        }

        public static IObservable<bool> IsLoggedIn(this IConnectionManager cm, IRepositoryHosts hosts, HostAddress address)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            Guard.ArgumentNotNull(address, nameof(address));
            return cm.Connections.ToObservable()
                    .Where(c => c.HostAddress.Equals(address))
                    .SelectMany(c => c.Login())
                    .Any(c => hosts.LookupHost(c.HostAddress).IsLoggedIn);
        }

        public static IObservable<bool> IsLoggedIn(this IConnection connection, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            return connection?.Login().Any(c => hosts.LookupHost(c.HostAddress).IsLoggedIn) ?? Observable.Return(false);
        }

        public static IObservable<IConnection> GetLoggedInConnections(this IConnectionManager cm, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            return cm.Connections.ToObservable()
                    .SelectMany(c => c.Login())
                    .Where(c => hosts.LookupHost(c.HostAddress).IsLoggedIn);
        }

        public static IObservable<IConnection> LookupConnection(this IConnectionManager cm, ILocalRepositoryModel repository)
        {
            return Observable.Return(repository?.CloneUrl != null
                ? cm.Connections.FirstOrDefault(c => c.HostAddress.Equals(HostAddress.Create(repository.CloneUrl)))
                : null);
        }
    }
}
