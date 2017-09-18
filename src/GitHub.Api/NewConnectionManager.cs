using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;

namespace GitHub.Api
{
    [Export(typeof(INewConnectionManager))]
    public class NewConnectionManager : INewConnectionManager
    {
        readonly IProgram program;
        readonly IConnectionCache cache;
        readonly IKeychain keychain;
        readonly ILoginManager loginManager;
        readonly TaskCompletionSource<object> loaded;
        readonly Lazy<ObservableCollection<INewConnection>> connections;

        [ImportingConstructor]
        public NewConnectionManager(
            IProgram program,
            IConnectionCache cache,
            IKeychain keychain,
            ILoginManager loginManager)
        {
            this.program = program;
            this.cache = cache;
            this.keychain = keychain;
            this.loginManager = loginManager;
            loaded = new TaskCompletionSource<object>();
            connections = new Lazy<ObservableCollection<INewConnection>>(
                CreateConnections,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ObservableCollection<INewConnection> Connections => connections.Value;

        public async Task<INewConnection> GetConnection(HostAddress address)
        {
            return (await GetLoadedConnections()).FirstOrDefault(x => x.HostAddress == address);
        }

        public async Task<ObservableCollection<INewConnection>> GetLoadedConnections()
        {
            var result = Connections;
            await loaded.Task;
            return result;
        }

        public async Task<INewConnection> LogIn(HostAddress address, string userName, string password)
        {
            var conns = await GetLoadedConnections();

            if (conns.Any(x => x.HostAddress == address))
            {
                throw new InvalidOperationException($"A connection to {address} already exists.");
            }

            var client = CreateClient(address);
            var user = await loginManager.Login(address, client, userName, password);
            var connection = new NewConnection(address, userName, user, null);
            conns.Add(connection);
            return connection;
        }

        public async Task LogOut(HostAddress address)
        {
            var connection = await GetConnection(address);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Could not find a connection to {address}.");
            }

            var client = CreateClient(address);
            await loginManager.Logout(address, client);
            Connections.Remove(connection);
        }

        ObservableCollection<INewConnection> CreateConnections()
        {
            var result = new ObservableCollection<INewConnection>();
            LoadConnections(result).Forget();
            return result;
        }

        async Task LoadConnections(ObservableCollection<INewConnection> result)
        {
            try
            {
                foreach (var c in await cache.Load())
                {
                    var client = CreateClient(c.HostAddress);
                    User user = null;
                    Exception error = null;

                    try
                    {
                        user = await loginManager.LoginFromCache(c.HostAddress, client);
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }

                    result.Add(new NewConnection(c.HostAddress, c.UserName, user, error));
                }
            }
            finally
            {
                loaded.SetResult(null);
            }
        }

        IGitHubClient CreateClient(HostAddress address)
        {
            return new GitHubClient(
                program.ProductHeader,
                new KeychainCredentialStore(keychain, address),
                address.ApiUri);
        }
    }
}
