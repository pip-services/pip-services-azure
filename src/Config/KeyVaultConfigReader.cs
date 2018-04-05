using PipServices.Azure.Auth;
using PipServices.Commons.Auth;
using PipServices.Commons.Config;
using PipServices.Commons.Connect;
using PipServices.Commons.Refer;
using System;

namespace PipServices.Azure.Config
{
    /// <summary>
    /// Reads configuration from Azure KeyVault secrets. Secret key becomes a parameter name
    /// </summary>
    public class KeyVaultConfigReader : CachedConfigReader, IReferenceable, IConfigurable
    {
        private ConnectionResolver _connectionResolver = new ConnectionResolver();
        private CredentialResolver _credentialResolver = new CredentialResolver();

        public KeyVaultConfigReader() { }

        public KeyVaultConfigReader(ConfigParams config)
        {
            if (config != null) Configure(config);
        }

        public virtual void SetReferences(IReferences references)
        {
            _connectionResolver.SetReferences(references);
            _credentialResolver.SetReferences(references);
        }

        public override void Configure(ConfigParams config)
        {
            base.Configure(config);
            _connectionResolver.Configure(config, true);
            _credentialResolver.Configure(config, true);
        }

        public static ConfigParams ReadConfiguration(string correlationId, ConfigParams config)
        {
            return new KeyVaultConfigReader(config).ReadConfig(correlationId, config);
        }

        public static ConfigParams ReadConfiguration(string correlationId, string connectionString)
        {
            var config = ConfigParams.FromString(connectionString);
            return new KeyVaultConfigReader(config).ReadConfig(correlationId, config);
        }

        protected override ConfigParams PerformReadConfig(string correlationId, ConfigParams parameters)
        {
            try
            {
                var connection = _connectionResolver.ResolveAsync(correlationId).Result;
                var credential = _credentialResolver.LookupAsync(correlationId).Result;
                KeyVaultClient _client = new KeyVaultClient(connection, credential);

                var secrets = _client.GetSecretsAsync().Result;
                var result = new ConfigParams();

                foreach (var entry in secrets)
                {
                    var key = entry.Key.Replace('-', '.');
                    var value = entry.Value;
                    result[key] = value;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to load config from KeyVault", ex);
            }
        }
    }
}
