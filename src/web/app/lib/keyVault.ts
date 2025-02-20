import { DefaultAzureCredential } from "@azure/identity";
import { SecretClient } from "@azure/keyvault-secrets";

export async function fetchKeyVaultSecret(): Promise<string> {
  const keyVaultUrl = process.env.KEY_VAULT_URL;
  const secretName = process.env.SECRET_NAME;

  if (!keyVaultUrl || !secretName) {
    console.warn(`Azure Key Vault configuration is missing:
      KEY_VAULT_URL: ${keyVaultUrl ? "✓" : "✗"}
      SECRET_NAME: ${secretName ? "✓" : "✗"}`);
    return "";
  }

  try {
    // Validate URL format
    const url = new URL(keyVaultUrl);
    if (!url.hostname.includes(".vault.azure.net")) {
      throw new Error(`Invalid Key Vault URL format: ${keyVaultUrl}`);
    }

    // Try Azure CLI credential first for local development
    let credential;
    try {
      credential = new DefaultAzureCredential({
        managedIdentityClientId: process.env.AZURE_CLIENT_ID,
      });
    } catch (error) {
      console.error("Failed to create Azure credential:", error);
      throw new Error("Azure authentication failed");
    }

    console.log("Attempting to fetch secret:", secretName);
    const client = new SecretClient(keyVaultUrl, credential);
    const secret = await client.getSecret(secretName);

    if (!secret?.value) {
      throw new Error(`Secret '${secretName}' value is empty`);
    }

    console.log("Successfully retrieved secret");
    return secret.value;
  } catch (error) {
    const errorMessage =
      error instanceof Error ? error.message : "Unknown error";
    console.error(`Failed to fetch secret from Azure Key Vault:
      URL: ${keyVaultUrl}
      Secret: ${secretName}
      Error: ${errorMessage}
    `);
    throw error;
  }
}
