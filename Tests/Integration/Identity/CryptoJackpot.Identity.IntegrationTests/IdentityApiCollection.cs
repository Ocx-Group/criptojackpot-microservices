using CryptoJackpot.Identity.IntegrationTests.Infrastructure;
using Xunit;
namespace CryptoJackpot.Identity.IntegrationTests;

[CollectionDefinition(nameof(IdentityApiCollection))]
public class IdentityApiCollection : ICollectionFixture<IdentityApiFactory>;