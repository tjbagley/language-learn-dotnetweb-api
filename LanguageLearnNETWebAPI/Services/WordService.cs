using LanguageLearnNETWebAPI.Models;
using LanguageLearnNETWebAPI.Repositories;

namespace LanguageLearnNETWebAPI.Services
{
    public class WordService : IWordService
    {
        private readonly ILogger<WordService> _logger;
        private readonly IWordRepository _wordRepository;

        public WordService(ILogger<WordService> logger, IWordRepository wordRepository)
        {
            _logger = logger;
            _wordRepository = wordRepository;
        }

        public async Task<Result<Word>> GetWord(string id)
        {
            return await _wordRepository.GetWord(id);
        }

        public async Task<Result<IList<Word>>> Search(string query)
        {
            return await _wordRepository.Search(query);
        }

        public async Task<Result<Word>> UpsertWord(Word word)
        {
            if (string.IsNullOrEmpty(word.Id))
            {
                word.Id = Guid.NewGuid().ToString();
            }
            return await _wordRepository.UpsertWord(word);
        }

        public async Task<Result<bool>> DeleteWord(string id)
        {
            return await _wordRepository.DeleteWord(id);
        }
    }
}
