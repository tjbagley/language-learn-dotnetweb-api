using LanguageLearnNETWebAPI.Models;

namespace LanguageLearnNETWebAPI.Repositories
{
    public interface IWordRepository
    {
        Task<Result<Word>> GetWord(string id);
        Task<Result<IList<Word>>> Search(string query);
        Task<Result<Word>> UpsertWord(Word word);
        Task<Result<bool>> DeleteWord(string id);
    }
}
