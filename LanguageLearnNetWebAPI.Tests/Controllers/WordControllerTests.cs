using AutoMapper;
using FluentAssertions;
using LanguageLearnNETWebAPI.Controllers;
using LanguageLearnNETWebAPI.Models;
using LanguageLearnNETWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LanguageLearnNetWebAPI.Tests.Controllers
{
    public class WordControllerTests
    {
        private readonly ILogger<WordController> _logger;
        private readonly IMapper _mapper;
        private readonly IWordService _wordService;
        private readonly WordController _controller;

        public WordControllerTests()
        {
            _logger = Substitute.For<ILogger<WordController>>();
            _mapper = Substitute.For<IMapper>();
            _wordService = Substitute.For<IWordService>();
            _controller = new WordController(_logger, _mapper, _wordService);
        }

        [Fact]
        public async Task Get_Returns_IActionResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var id = "w1";
            var word = new Word { Id = id, Value = "hello", Meaning = "greeting" };
            _wordService.GetWord(id).Returns(Task.FromResult(new Result<Word>(word)));

            // Act
            IActionResult result = await _controller.Get(id);

            // Assert - result is IActionResult and contains the expected value
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            ok.Value.Should().BeSameAs(word);
        }

        [Fact]
        public async Task Get_Returns_IActionResult_With_StatusCode_WhenServiceReturnsError()
        {
            // Arrange
            var id = "missing";
            _wordService.GetWord(id).Returns(Task.FromResult(new Result<Word>(404, "Not found")));

            // Act
            IActionResult result = await _controller.Get(id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var obj = (ObjectResult)result;
            obj.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Search_Returns_IActionResult_With_List_WhenServiceReturnsSuccess()
        {
            // Arrange
            var query = "he";
            var list = new List<Word> { new Word { Id = "1", Value = "hello", Meaning = "greeting" } };
            _wordService.Search(query).Returns(Task.FromResult(new Result<IList<Word>>(list)));

            // Act
            IActionResult result = await _controller.Search(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            ok.Value.Should().BeSameAs(list);
        }

        [Fact]
        public async Task Post_Returns_BadRequest_IActionResult_When_ModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Value", "Required");
            var wordBase = new WordBase { Value = string.Empty, Meaning = string.Empty };

            // Act
            IActionResult result = await _controller.Post(wordBase);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Post_Calls_Mapper_And_Service_And_Returns_IActionResult()
        {
            // Arrange
            var wordBase = new WordBase { Value = "hi", Meaning = "greeting" };
            var mappedWord = new Word { Id = "new", Value = "hi", Meaning = "greeting" };

            _mapper.Map<Word>(wordBase).Returns(mappedWord);
            _wordService.UpsertWord(mappedWord).Returns(Task.FromResult(new Result<Word>(mappedWord)));

            // Act
            IActionResult result = await _controller.Post(wordBase);

            // Assert - service and mapper called and IActionResult contains expected value
            await _wordService.Received(1).UpsertWord(mappedWord);
            _mapper.Received(1).Map<Word>(wordBase);

            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            ok.Value.Should().BeSameAs(mappedWord);
        }

        [Fact]
        public async Task Put_Returns_BadRequest_IActionResult_When_ModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Required");
            var word = new Word { Id = null, Value = "x", Meaning = "m" };

            // Act
            IActionResult result = await _controller.Put(word);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Put_Calls_Service_And_Returns_IActionResult()
        {
            // Arrange
            var word = new Word { Id = "existing", Value = "word", Meaning = "def" };
            _wordService.UpsertWord(word).Returns(Task.FromResult(new Result<Word>(word)));

            // Act
            IActionResult result = await _controller.Put(word);

            // Assert
            await _wordService.Received(1).UpsertWord(word);

            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            ok.Value.Should().BeSameAs(word);
        }

        [Fact]
        public async Task Delete_Returns_IActionResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var id = "to-delete";
            _wordService.DeleteWord(id).Returns(Task.FromResult(new Result<bool>(true)));

            // Act
            IActionResult result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            ok.Value.Should().Be(true);
        }

        [Fact]
        public async Task Delete_Returns_IActionResult_With_StatusCode_WhenServiceReturnsError()
        {
            // Arrange
            var id = "missing";
            _wordService.DeleteWord(id).Returns(Task.FromResult(new Result<bool>(404, "Not found")));

            // Act
            IActionResult result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var obj = (ObjectResult)result;
            obj.StatusCode.Should().Be(404);
        }
    }
}