﻿using Microsoft.IdentityModel.Clients.ActiveDirectory;
using PipServices.Commons.Auth;
using PipServices.Commons.Connect;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PipServices.Azure.Auth
{
    public class KeyVaultClient
    {
        private string _keyVault;
        private string _clientId;
        private string _clientKey;
        private string _thumbPrint;
        private Microsoft.Azure.KeyVault.KeyVaultClient _client;

        public KeyVaultClient(ConnectionParams connection, CredentialParams credential)
        {
            _keyVault = connection.GetAsNullableString("key_vault") 
                ?? connection.GetAsNullableString("uri") 
                ?? connection.GetAsNullableString("KeyVault");
            if (_keyVault == null)
                throw new ArgumentNullException("KeyVault parameter is not defined");
            if (!_keyVault.StartsWith("http"))
                _keyVault = "https://" + _keyVault + ".vault.azure.net";

            _clientId = credential.AccessId ?? credential.GetAsNullableString("ClientId");
            if (_clientId == null)
                throw new ArgumentNullException("CliendId parameter is not defined");

            _clientKey = credential.AccessKey ?? credential.GetAsNullableString("ClientKey");
            _thumbPrint = credential.GetAsNullableString("thumbprint")
                ?? credential.GetAsNullableString("ThumbPrint");
            if (_clientKey == null && _thumbPrint == null)
                throw new ArgumentNullException("Neither ClientKey or ThumbPrint parameters are not defined");

            _client = new Microsoft.Azure.KeyVault.KeyVaultClient(
                new Microsoft.Azure.KeyVault.KeyVaultClient.AuthenticationCallback(GetAccessToken));
        }

        public async Task<List<string>> GetSecretNamesAsync()
        {
            var message = await _client.GetSecretsAsync(_keyVault, null);
            var result = new List<string>();

            foreach (var item in message.Value)
            {
                result.Add(item.Identifier.Name);
            }

            return result;
        }

        public async Task<string> GetSecretValueAsync(string secretName)
        {
            var message = await _client.GetSecretAsync(_keyVault + "/secrets/" + secretName);
            return message.Value;
        }

        public async Task<Dictionary<string, string>> GetSecretsAsync()
        {
            var secretNames = await GetSecretNamesAsync();
            var result = new Dictionary<string, string>();
            foreach (var secretName in secretNames)
            {
                var secretValue = await GetSecretValueAsync(secretName);
                result[secretName] = secretValue;
            }
            return result;
        }

        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            if (_clientKey != null)
            {
                var clientCredential = new ClientCredential(_clientId, _clientKey);
                var result = await context.AcquireTokenAsync(resource, clientCredential);
                return result.AccessToken;
            }
            else
            {
                var clientAssertionCertPfx = FindCertificateByThumbprint(_thumbPrint);
                var cert = new ClientAssertionCertificate(_clientId, clientAssertionCertPfx);
                var result = await context.AcquireTokenAsync(resource, cert);
                return result.AccessToken;
            }
        }

        public static X509Certificate2 FindCertificateByThumbprint(string thumbPrint)
        {
            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    thumbPrint,
                    false
                ); // Don't validate certs, since the test root isn't installed.
                if (certCollection == null || certCollection.Count == 0)
                    return null;
                return certCollection[0];
            }
            finally
            {
                certStore.Close();
            }
        }
    }
}
