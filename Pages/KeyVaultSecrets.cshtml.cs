﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace demo.Pages
{
    public class KeyVaultSecretsModel : PageModel
    {
        private readonly ILogger<KeyVaultSecretsModel> _logger;
        private readonly IConfiguration configuration;
        public string ConfigSetting { get; set; }

        public KeyVaultSecretsModel(ILogger<KeyVaultSecretsModel> logger, IConfiguration config)
        {
            configuration = config;
            _logger = logger;
        }

        public string SecretValue { get; set; }
        public async Task OnGetAsync()
        {
            SecretValue = "Secret value is currently empty";
            ConfigSetting = configuration["SomeConfigValueFromKV"];
            try
            {
                var userAssignedId = configuration["UserAssignedId"];

                AzureServiceTokenProvider azureServiceTokenProvider;

                if (string.IsNullOrEmpty(userAssignedId))
                {
                    azureServiceTokenProvider = new AzureServiceTokenProvider();
                }
                else
                {
                    azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={userAssignedId}");
                }

                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync("https://[YOUR_KEY_VAULT_URL]]secrets/[SECRET_NAME]")
                        .ConfigureAwait(false);
                SecretValue = secret.Value;
            }

            catch (KeyVaultErrorException keyVaultException)
            {
                SecretValue = keyVaultException.Message;
            }
        }
    }
}
