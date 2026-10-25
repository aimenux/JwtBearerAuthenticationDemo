# 🎟️ JwtAuthenticationDemo

> Twelve small ASP.NET Core Web API projects that protect the same movies API with a **JWT bearer token** in the `Authorization` header, each using a different authentication mechanism.

> [!WARNING]
> ⚠️ **This is a demo.** The examples validate tokens signed with a **shared symmetric key** (HMAC-SHA256) that is committed to source control. Anyone holding that key can mint tokens.
>
> 🔒 **Always use it over HTTPS.** For real systems, tokens should be issued by an identity provider (OpenID Connect / OAuth 2.0) and validated with asymmetric keys.

## 📑 Table of contents

- [🎯 What is this?](#-what-is-this)
- [🔒 Why HTTPS is mandatory](#-why-https-is-mandatory)
- [🚀 Quick start](#-quick-start)
- [🧩 Examples at a glance](#-examples-at-a-glance)
- [🛡️ Security posture](#️-security-posture)
- [🧪 Testing](#-testing)
- [🏭 Going to production](#-going-to-production)
- [🗂️ Project structure](#️-project-structure)
- [🛠️ Tools](#️-tools)

## 🎯 What is this?

Each `ExampleNN` project exposes the same two endpoints and protects them with a JWT bearer token:

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/movies/list` | Returns all movies |
| `GET` | `/api/movies/{movieId}` | Returns one movie (`404` if unknown) |

The token is read from the `Authorization: Bearer <token>` request header and validated against the `Authentication` configuration:

- **Signature** with `HS256` (a shared key in Examples 01–08, 11, 12) or `RS256` (an RSA public key in Examples 09, 10). `alg: none` and other algorithms are rejected.
- **Issuer** (`Authentication:Issuer`) and **audience** (`Authentication:Audience`)
- **Lifetime** (`exp` / `nbf`)

A missing, malformed, or invalid token returns `401 Unauthorized` with `WWW-Authenticate: Bearer`.

The examples differ in **how** the token is checked:

- Controllers or minimal APIs
- Middleware, authorization filters, endpoint filters, or a custom authentication scheme
- Global, per route group, per action, or per route

All of them share the same validation logic, held by a `JwtTokenValidator` in each project.

## 🔒 Why HTTPS is mandatory

A bearer token grants access to whoever presents it. Over plain HTTP it is visible to anyone on the network path, so it can be captured and replayed.

Each example therefore only listens on HTTPS:

- `WebApplication.CreateBuilder(args, enforceHttps: true)` configures Kestrel to listen on the HTTPS port only. Plain-HTTP clients are refused.
- The port comes from `Https:Port` in configuration, or from the `https` entry in `urls`. If neither is set, startup fails.
- `UseHttpsRedirection()` redirects any HTTP request to HTTPS, and `UseHsts()` is enabled outside Development.

## 🚀 Quick start

### 1️⃣ Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) matching `global.json` (`10.0.x`)
- A trusted development certificate (`dotnet dev-certs https --trust`)

### 2️⃣ Run an example

```bash
dotnet run --project src/Example01
```

Each example uses its own HTTPS port (`5001`–`5012`, see the table below). The port and the token settings are in `appsettings.Development.json`:

```json
{
  "Https": {
    "Port": 5001
  },
  "Authentication": {
    "Issuer": "JwtAuthenticationDemo",
    "Audience": "MoviesApi",
    "SigningKey": "dev-only-signing-key-do-not-use-in-production"
  }
}
```

`SigningKey` must be at least 32 characters. The app refuses to start otherwise.

> 🔌 Examples can run side by side because each one has its own port.

### 3️⃣ Call the API

A ready-made development token (valid for 10 years, signed with the development key above) is stored in `@token` in each `test/ExampleNN.Tests/ExampleNN.Tests.http` file. Copy it into a shell variable:

```bash
TOKEN="<paste the @token value from test/Example01.Tests/Example01.Tests.http>"

# ✅ Valid token over HTTPS
curl -k -H "Authorization: Bearer $TOKEN" https://localhost:5001/api/movies/list

# ❌ No token → 401 Unauthorized
curl -k https://localhost:5001/api/movies/list

# ❌ Malformed or invalid token → 401 Unauthorized
curl -k -H "Authorization: Bearer abc.def.ghi" https://localhost:5001/api/movies/list

# ❌ Plain HTTP → connection refused (the server only listens on HTTPS)
curl http://localhost:5001/api/movies/list
```

> The development key is committed for convenience. Never reuse it outside your machine.

### 🪙 Creating your own tokens

`src/JwtTokenGenerator` prints a signed token with the same issuer, audience, and development key as the examples:

```bash
dotnet run --project src/JwtTokenGenerator
dotnet run --project src/JwtTokenGenerator -- alice 15 RS256
```

The optional arguments are the subject and the lifetime in minutes. When omitted, they come from `src/JwtTokenGenerator/appsettings.json`:

```json
{
  "Authentication": {
    "Issuer": "JwtAuthenticationDemo",
    "Audience": "MoviesApi",
    "SigningKey": "dev-only-signing-key-do-not-use-in-production"
  },
  "TokenGenerator": {
    "DefaultSubject": "demo-user",
    "DefaultLifetimeMinutes": 60
  }
}
```

The `Authentication` values must match the examples' `appsettings.Development.json`, or the tokens will be rejected. `RS256` tokens also need `TokenGenerator:RsaPrivateKeyPem`, the private half of the PEM key that Examples 09 and 10 use. The tool only creates development tokens.

### 🔎 Inspecting a token

`src/JwtTokenInspector` prints the header, payload, subject, issuer, audience, and validity window of a token. It then validates the token with the same issuer, audience, and key as the examples. Each algorithm is checked separately: HS256 against `SigningKey` (Examples 01–08, 11), RS256 against `PublicKeyPem` (Examples 09–10). A token is reported valid only by the family that accepts its `alg`; other families are reported as rejected:

```bash
dotnet run --project src/JwtTokenInspector -- <token>
```

Exit codes: `0` valid, `2` decoded but invalid (the reason is printed), `1` usage error or not a JWT. Its `appsettings.json` must match the examples' `Authentication` section.

## 🧩 Examples at a glance

| # | 🧱 Style | 🔑 Mechanism | 📍 Scope | ❓ Status Code | 🔌 Port |
|---|----------|--------------|----------|------------------|---------|
| 01 | Controllers | Middleware | Global | **401** | `5001` |
| 02 | Minimal API | Middleware | Global | **401** | `5002` |
| 03 | Controllers | Authorization filter | Global | 404 | `5003` |
| 04 | Minimal API | Endpoint filter | Per route group | 404 | `5004` |
| 05 | Controllers | Authorization filter | Per action (attribute) | 404 | `5005` |
| 06 | Minimal API | Endpoint filter | Per route | 404 | `5006` |
| 07 | Controllers | Custom authentication scheme | Per route group | 404 | `5007` |
| 08 | Minimal API | Custom authentication scheme | Per route | 404 | `5008` |
| 09 | Controllers | Asymmetric RS256 (PEM public key) on custom scheme | Global | 401 | `5009` |
| 10 | Minimal API | Asymmetric RS256 (PEM public key) on custom scheme | Per route group | 401 | `5010` |
| 11 | Controllers | Built-in JWT Bearer handler | Global | 401 | `5011` |
| 12 | Minimal API | Built-in JWT Bearer handler | Per route group | 401 | `5012` |

### 🔍 Reading the table

- **Middleware (01, 02)** runs for every request, so unknown routes are also protected and return `401`.
- **Filters and schemes (03–08)** run only for matched endpoints, so unknown routes return `404` as usual.
- **Asymmetric signing (09, 10)** verifies tokens with an RSA public key (`Authentication:PublicKeyPem`, PEM format) instead of a shared secret. The server can check tokens but cannot issue them. These examples use the same custom scheme as Examples 07 and 08.
- **Built-in handler (11, 12)** uses `AddJwtBearer()`, the framework's own JWT validation. It is the production-style option. These examples have no authorization policy, so any valid token passes.
- **Protection by default:** route-group and global filters (03, 04) protect routes added later to that scope. Per-route attributes and filters (05, 06, 08) must be repeated on each new route, so a new route is public until it is protected.

### 🧭 Which one should I read first?

1. **Example01** for the simplest version: a middleware that validates the bearer token.
2. **Example03** to see the same check as an authorization filter.
3. **Example07** for a custom authentication scheme, which is the most "framework-like" option.

## 🛡️ Security posture

| ✅ Measure | Where | Why it matters |
|-----------|-------|----------------|
| 🔐 **HTTPS-only listener** | `WebApplication.CreateBuilder(args, enforceHttps: true)` in each `Program.cs` | Kestrel never opens an HTTP socket, so plain-HTTP clients are refused |
| 🔢 **Configurable HTTPS port** | `Https:Port` in `appsettings.Development.json` | No port hardcoded in code; the HTTPS port is set in configuration |
| ⏳ **HSTS** | `app.UseHsts()` | Browsers that have visited over HTTPS will upgrade future requests |
| 🚫 **Fail-closed startup** | `JwtOptionsValidator` (`IValidateOptions<JwtOptions>`) with `ValidateOnStart()` | The app refuses to start, naming each missing field, when the issuer, audience, or a signing key of at least 32 characters is missing |
| 🔏 **Algorithm pinning** | `ValidAlgorithms = [HmacSha256]` in `JwtTokenValidator` | Tokens signed with `none` or any other algorithm are rejected |
| 🧾 **Issuer, audience, lifetime checks** | `TokenValidationParameters` in `JwtTokenValidator` | A token for another API or issuer, or an expired one, is rejected |
| 🧾 **Typed configuration** | `JwtOptions` bound to `Authentication` | Consumers read a validated `IOptions<JwtOptions>` instead of raw strings |
| 🪪 **`WWW-Authenticate: Bearer`** | Every `401` response | Clients can discover the scheme from the challenge header |

### ⚠️ Known limitations

These are intentionally left out of the demo so the examples stay short:

- **Symmetric signing key (Examples 01–08, 11, 12).** The same key signs and verifies tokens, so every service that validates tokens can also mint them. Examples 09 and 10 use an RSA public key, so they can verify tokens without being able to mint them.
- **Plaintext key in source control.** `appsettings.Development.json` contains the development signing key.
- **No revocation or refresh.** A valid token works until it expires.

## 🔧 Implementation notes

- **`JwtTokenValidator.Authenticate`** returns the `ClaimsPrincipal` of a valid `Authorization: Bearer` token, or `null` when the header is missing, is not a bearer header, or the token fails validation. Each mechanism turns `null` into its own `401` response.
- **Startup refusal tests** expect either `OptionsValidationException` or `ObjectDisposedException`. `WebApplicationFactory` can surface the disposal exception instead of the validation error when the host fails during startup, so either one means startup was refused.

## 🧪 Testing

```bash
dotnet test
```

Each `ExampleNN.Tests` project covers the same behaviour:

| 🧪 Test | What it checks |
|--------|----------------|
| `WebApiTests` | Valid token returns `200`; wrong signature, expired token, wrong audience, missing or malformed `Authorization` header return `401`; missing or too-short signing key stops startup |
| `JwtMiddlewareTests` / `JwtSecurityFilterTests` / `JwtAuthenticationHandlerTests` | Unit tests for the mechanism of each example |
| `ExampleNN.Tests` (09, 10) | `TestTokens` generates an RSA key pair in memory, and the startup test accepts only a valid PEM public key |
| `MovieRepositoryTests` | The returned movies cannot be cast to a mutable list or changed |
| `MoviesControllerTests` / `MoviesEndpointsTests` | The HTTP result of each endpoint, using a mocked repository |

Integration tests run the real pipeline in memory with `WebApplicationFactory`. They use the HTTPS base address `https://localhost:5000` and do not bind any port. `TestTokens` mints the tokens they send.

- `WebApiTransportTests` start the app's real Kestrel listener on a free port, which the in-memory tests do not cover. Kestrel closes a plain-HTTP request sent to the TLS-only port without an HTTP response, so the test expects the request to fail. HTTPS needs the ASP.NET Core development certificate (`dotnet dev-certs https --trust`). The test client accepts it without validation.
- `WebApiTokenRejectionTests` send forged tokens: `alg: none`, a modified payload with the original signature, a wrong issuer, a non-Bearer scheme, and a `Bearer` header with no token. Examples 01–08, 11, and 12 also reject an RS256 token. Examples 09 and 10 also reject an HS256 token keyed with the public PEM, the classic algorithm-confusion attack.
- `WebApiRouteTests` check the response bodies of both routes and the `404` for an unknown id and a non-integer id.

## 🏭 Going to production

This demo validates tokens itself to show where the check lives in each mechanism. A real API should:

| Option | Use it when... |
|--------|----------------|
| 🎟️ **`Microsoft.AspNetCore.Authentication.JwtBearer`** with `AddJwtBearer()` | You need standard JWT bearer validation with metadata discovery and key rotation |
| 🪪 **OAuth 2.0 / OpenID Connect** (Microsoft Entra ID, Auth0, Keycloak, ...) | You need centralized login, SSO, scopes, or third-party clients |
| 🔑 **Asymmetric signing (RS256 / ES256)** with keys published via JWKS | Services must validate tokens without being able to issue them |
| 🧱 **Mutual TLS** | You control both client and server and need strong client identity |

### 🔑 JWKS or PEM?

This demo reads an RSA public key in PEM format from `Authentication:PublicKeyPem`. That keeps the example self-contained, with no network call and no metadata endpoint, and makes it easy to test with an in-memory key pair.

**For production, use JWKS instead of PEM.** A JSON Web Key Set is published by the identity provider over HTTPS. The token's `kid` header selects the key, and the set can rotate keys without restarting the API. A PEM key is one static value in configuration: rotating it means a deployment, and during the switch some valid tokens are rejected.

JWKS costs more to run: the API needs a reachable endpoint, caching, timeouts, and a plan for when the endpoint is down. With the built-in handler this is one line, `AddJwtBearer()` with an `Authority`, which fetches the metadata and keys for you.

Also keep the signing key (or the identity provider's metadata address) in configuration or a secret store, never in source control.

## 🗂️ Project structure

```text
JwtBearerAuthenticationDemo/
├── 📁 src/
│   ├── 📁 JwtTokenGenerator/            # Console app that prints a dev token
│   ├── 📁 JwtTokenInspector/            # Console app that decodes and validates a token
│   ├── 📁 Example01/                   # Controllers + middleware
│   ├── 📁 Example02/                   # Minimal API + middleware
│   ├── 📁 Example03/                   # Controllers + global authorization filter
│   ├── 📁 Example04/                   # Minimal API + route group endpoint filter
│   ├── 📁 Example05/                   # Controllers + per-action authorization filter
│   ├── 📁 Example06/                   # Minimal API + per-route endpoint filter
│   ├── 📁 Example07/                   # Controllers + custom authentication scheme
│   ├── 📁 Example08/                   # Minimal API + custom authentication scheme
│   ├── 📁 Example09/                   # Controllers + custom scheme + RS256 public key
│   ├── 📁 Example10/                   # Minimal API + custom scheme + RS256 public key
│   ├── 📁 Example11/                   # Controllers + built-in JWT Bearer handler
│   └── 📁 Example12/                   # Minimal API + built-in JWT Bearer handler
├── 📁 test/                            # One xUnit project per example
├── 📄 Directory.Build.props            # Shared build settings
├── 📄 Directory.Packages.props         # Central package versions
└── 📄 global.json                      # Pinned .NET SDK
```

## 🛠️ Tools

| Tool | Purpose |
|------|---------|
| 🟣 **.NET 10 / ASP.NET Core** | Web API, Kestrel, authentication and authorization |
| 🎟️ **Microsoft.AspNetCore.Authentication.JwtBearer** | Built-in JWT Bearer handler (Examples 11 and 12) |
| 🎟️ **Microsoft.IdentityModel.JsonWebTokens** | JWT validation (`JsonWebTokenHandler`) and token creation in tests |
| 📘 **Swagger / Swashbuckle** | Interactive API docs with an HTTP bearer security scheme (Development only) |
| 🧪 **xUnit** | Test framework |
| ✨ **AwesomeAssertions** | Readable assertions |
| 🎭 **NSubstitute** | Mocking repositories |

---

<p align="center">
  <sub>Made for learning 📚 · Not for production 🚫 · Always use HTTPS 🔒</sub>
</p>
