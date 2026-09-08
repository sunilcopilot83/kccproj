# ProvisionService

Simple ASP.NET Core microservice that implements POST /api/v1/provision.

Request:
{
  "device_id": "AA:BB:CC:DD:EE:FF",
  "bootstrap_token": "<hex token>",
  "csr": "-----BEGIN CERTIFICATE REQUEST-----...-----END CERTIFICATE REQUEST-----"
}

Response (success 200):
{
  "device_certificate": "-----BEGIN CERTIFICATE-----...-----END CERTIFICATE-----"
}

Run locally:
- cd ProvisionService
- dotnet restore
- dotnet run

Test (example curl -- replace CSR & token):
curl -X POST http://localhost:5000/api/v1/provision \
  -H "Content-Type: application/json" \
  -d '{"device_id":"AA:BB:CC:DD:EE:FF","bootstrap_token":"012345...","csr":"-----BEGIN CERTIFICATE REQUEST-----\n...\n-----END CERTIFICATE REQUEST-----\n"}'

Notes:
- Uses BouncyCastle to parse CSR and sign with Ed25519 CA.
- If ca.key.pem / ca.cert.pem are absent, a self-signed ED25519 CA is generated and written if paths are set in appsettings.json.
- For production, store CA private key securely (Azure Key Vault/HSM).
