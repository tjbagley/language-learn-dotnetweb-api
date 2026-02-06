using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LanguageLearnNETWebAPI.Models;
using LanguageLearnNETWebAPI.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageLearnNetWebAPI.Tests.Integration
{
    public class WordControllerIntegrationTests : IClassFixture<IntegrationTestFactory>
    {
        private readonly IntegrationTestFactory _factory;

        public WordControllerIntegrationTests(IntegrationTestFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_Get_Delete_Workflow()
        {
            var client = _factory.CreateClient();

            // Create
            var createResp = await client.PostAsJsonAsync("/word", new { Value = "hello", Meaning = "greeting" });
            createResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await createResp.Content.ReadFromJsonAsync<Word>();
            created.Should().NotBeNull();
            created!.Value.Should().Be("hello");
            created.Id.Should().NotBeNullOrEmpty();

            // Get
            var getResp = await client.GetAsync($"/word/{created.Id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var got = await getResp.Content.ReadFromJsonAsync<Word>();
            got.Should().NotBeNull();
            got!.Id.Should().Be(created.Id);

            // Delete
            var delResp = await client.DeleteAsync($"/word/{created.Id}");
            delResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var delResult = await delResp.Content.ReadFromJsonAsync<bool>();
            delResult.Should().BeTrue();

            // Ensure deleted returns 404
            var missingResp = await client.GetAsync($"/word/{created.Id}");
            missingResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Search_Returns_Matching_Items()
        {
            var client = _factory.CreateClient();

            // seed a couple items
            var a = await client.PostAsJsonAsync("/word", new { Value = "alpha", Meaning = "first" });
            var b = await client.PostAsJsonAsync("/word", new { Value = "beta", Meaning = "second" });

            a.StatusCode.Should().Be(HttpStatusCode.OK);
            b.StatusCode.Should().Be(HttpStatusCode.OK);

            var searchResp = await client.GetAsync("/word/search?query=alph");
            searchResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await searchResp.Content.ReadFromJsonAsync<IList<Word>>();
            list.Should().NotBeNull();
            list!.Any(w => w.Value.Equals("alpha", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }
    }

    public class IntegrationTestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // replace IWordRepository with an in-memory test implementation
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWordRepository));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IWordRepository, InMemoryWordRepository>();
            });
        }
    }

    // Simple in-memory repository used only for integration tests
    internal class InMemoryWordRepository : IWordRepository
    {
        private readonly ConcurrentDictionary<string, Word> _store = new();

        public Task<Result<Word>> GetWord(string id)
        {
            if (string.IsNullOrEmpty(id) || !_store.TryGetValue(id, out var w))
            {
                return Task.FromResult(new Result<Word>(StatusCodes.Status404NotFound, "Word not found"));
            }

            return Task.FromResult(new Result<Word>(w));
        }

        public Task<Result<IList<Word>>> Search(string query)
        {
            var results = _store.Values
                .Where(w => string.IsNullOrEmpty(query) ||
                    (w.Value?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                    (w.Meaning?.Contains(query, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            return Task.FromResult(new Result<IList<Word>>(results));
        }

        public Task<Result<Word>> UpsertWord(Word word)
        {
            if (string.IsNullOrEmpty(word.Id))
            {
                return Task.FromResult(new Result<Word>(StatusCodes.Status400BadRequest, "Missing Id"));
            }

            _store.AddOrUpdate(word.Id, word, (_, __) => word);
            return Task.FromResult(new Result<Word>(word));
        }

        public Task<Result<bool>> DeleteWord(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Task.FromResult(new Result<bool>(StatusCodes.Status400BadRequest, "Missing Id"));
            }

            var removed = _store.TryRemove(id, out _);
            if (!removed)
            {
                return Task.FromResult(new Result<bool>(StatusCodes.Status404NotFound, "Word not found"));
            }

            return Task.FromResult(new Result<bool>(true));
        }
    }
}