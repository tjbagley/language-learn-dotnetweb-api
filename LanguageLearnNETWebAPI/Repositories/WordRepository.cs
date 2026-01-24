using AutoMapper;
using Azure;
using Azure.Data.Tables;
using LanguageLearnNETWebAPI.Models;
using LanguageLearnNETWebAPI.Models.AppSettings;
using Microsoft.Extensions.Options;
using System.Runtime;

namespace LanguageLearnNETWebAPI.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly string wordTableNAme = "Words";

        private readonly ILogger<WordRepository> _logger;
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private readonly APISettings _settings;

        public WordRepository(ILogger<WordRepository> logger, IMapper mapper, IConfiguration configuration, IOptions<APISettings> settings)
        {
            _logger = logger;
            _mapper = mapper;
            _connectionString = configuration.GetConnectionString("LanguageLearnStorageAccount")
                ?? throw new InvalidOperationException("Connection string not found.");
            _settings = settings.Value;
        }

        public async Task<Result<Word>> GetWord(string id)
        {
            try
            {
                var tableClient = new TableClient(_connectionString, wordTableNAme);

                var result = await tableClient.GetEntityAsync<WordEntity>(id, id);

                if (result?.HasValue != true)
                {
                    return new Result<Word>(StatusCodes.Status404NotFound, "Word not found");
                }

                var word = _mapper.Map<Word>(result.Value);
                return new Result<Word>(word);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == StatusCodes.Status404NotFound)
                {
                    _logger.LogError(ex, "Word not found: {id}", id);
                    return new Result<Word>(ex.Status, $"Word not found");
                }
                else
                {
                    _logger.LogError(ex, "Error getting word: {id}", id);
                    return new Result<Word>(ex.Status, $"Error getting word: {ex.Message}");
                }
            }
        }

        public async Task<Result<IList<Word>>> Search(string query)
        {
            try
            {
                var tableClient = new TableClient(_connectionString, wordTableNAme);

                List<Word> results = [];
                await foreach (var entity in tableClient.QueryAsync<WordEntity>())
                {
                    if (!string.IsNullOrEmpty(query) &&
                        (entity.Value?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                        entity.Meaning?.Contains(query, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        results.Add(_mapper.Map<Word>(entity));
                    }
                }

                if (results.Count > _settings.MaxWordSearchResults)
                {
                    results = [.. results.Take(_settings.MaxWordSearchResults)];
                }

                return new Result<IList<Word>>(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for words with: {query}", query);
                return new Result<IList<Word>>(StatusCodes.Status500InternalServerError, $"Error searching for words: {ex.Message}");
            }
        }

        public async Task<Result<Word>> UpsertWord(Word word)
        {
            try
            {
                if (string.IsNullOrEmpty(word.Id))
                {
                    return new Result<Word>(StatusCodes.Status400BadRequest, "Missing Id");
                }

                var entity = _mapper.Map<WordEntity>(word);
                var tableClient = new TableClient(_connectionString, wordTableNAme);

                var result = await tableClient.UpsertEntityAsync(entity);

                if (result.IsError)
                {
                    return new Result<Word>(result.Status, "Upsert failed");
                }
                else
                {
                    return new Result<Word>(word);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting word: {@word}", word);
                return new Result<Word>(StatusCodes.Status500InternalServerError, $"Error upserting word: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteWord(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new Result<bool>(StatusCodes.Status400BadRequest, "Missing Id");
                }

                var tableClient = new TableClient(_connectionString, wordTableNAme);

                var result = await tableClient.DeleteEntityAsync(id, id);

                if (result.IsError)
                {
                    return new Result<bool>(result.Status, "Delete failed");
                }
                else
                {
                    return new Result<bool>(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting word: {id}", id);
                return new Result<bool>(StatusCodes.Status500InternalServerError, $"Error deleting word: {ex.Message}");
            }
        }
    }
}
