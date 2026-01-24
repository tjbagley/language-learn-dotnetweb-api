using FluentAssertions;
using LanguageLearnNETWebAPI.Models;
using LanguageLearnNETWebAPI.Repositories;
using LanguageLearnNETWebAPI.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LanguageLearnNetWebAPI.Tests.Services
{
    public class WordServiceTests
    {
        private readonly IWordRepository _wordRepository;
        private readonly ILogger<WordService> _logger;
        private readonly WordService _service;

        public WordServiceTests()
        {
            _wordRepository = Substitute.For<IWordRepository>();
            _logger = Substitute.For<ILogger<WordService>>();
            _service = new WordService(_logger, _wordRepository);
        }

        [Fact]
        public async Task GetWord_ReturnsWord_FromRepository()
        {
            // Arrange
            var id = "word-1";
            var expected = new Word { Id = id, Value = "hello", Meaning = "greeting" };
            _wordRepository.GetWord(id).Returns(Task.FromResult(new Result<Word>(expected)));

            // Act
            var result = await _service.GetWord(id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeSameAs(expected);
            await _wordRepository.Received(1).GetWord(id);
        }

        [Fact]
        public async Task GetWord_ReturnsError_WhenRepositoryReturnsError()
        {
            // Arrange
            var id = "missing";
            _wordRepository.GetWord(id).Returns(Task.FromResult(new Result<Word>(404, "Not found")));

            // Act
            var result = await _service.GetWord(id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            await _wordRepository.Received(1).GetWord(id);
        }

        [Fact]
        public async Task Search_ReturnsList_FromRepository()
        {
            // Arrange
            var query = "he";
            var list = new List<Word> { new Word { Id = "1", Value = "hello", Meaning = "greeting" } };
            _wordRepository.Search(query).Returns(Task.FromResult(new Result<IList<Word>>(list)));

            // Act
            var result = await _service.Search(query);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(list);
            await _wordRepository.Received(1).Search(query);
        }

        [Fact]
        public async Task UpsertWord_WithExistingId_PassesThroughToRepository()
        {
            // Arrange
            var word = new Word { Id = "existing-id", Value = "word", Meaning = "definition" };
            _wordRepository.UpsertWord(word).Returns(Task.FromResult(new Result<Word>(word)));

            // Act
            var result = await _service.UpsertWord(word);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be("existing-id");
            await _wordRepository.Received(1).UpsertWord(word);
        }

        [Fact]
        public async Task UpsertWord_WithoutId_AssignsGuidAndCallsRepository()
        {
            // Arrange
            var word = new Word { Id = null, Value = "new", Meaning = "meaning" };

            // Make repository return the same Word instance it was called with
            _wordRepository.UpsertWord(Arg.Any<Word>())
                .Returns(ci => Task.FromResult(new Result<Word>(ci.Arg<Word>())));

            // Act
            var result = await _service.UpsertWord(word);

            // Assert           
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().NotBeNullOrEmpty();
            // Ensure repository was called with the same instance and it had an Id assigned
            await _wordRepository.Received(1).UpsertWord(Arg.Is<Word>(w => !string.IsNullOrEmpty(w.Id) && ReferenceEquals(w, word)));
        }

        [Fact]
        public async Task DeleteWord_ReturnsTrue_FromRepository()
        {
            // Arrange
            var id = "to-delete";
            _wordRepository.DeleteWord(id).Returns(Task.FromResult(new Result<bool>(true)));

            // Act
            var result = await _service.DeleteWord(id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            await _wordRepository.Received(1).DeleteWord(id);
        }
    }
}
