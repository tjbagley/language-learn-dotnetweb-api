using AutoMapper;
using LanguageLearnNETWebAPI.Models;
using LanguageLearnNETWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using LanguageLearnNETWebAPI.Extensions;

namespace LanguageLearnNETWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly ILogger<WordController> _logger;
        private readonly IMapper _mapper;
        private readonly IWordService _wordService;

        public WordController(ILogger<WordController> logger, IMapper mapper, IWordService wordService)
        {
            _logger = logger;
            _mapper = mapper;
            _wordService = wordService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _wordService.GetWord(id);

            return this.HandleResult(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _wordService.Search(query);

            return this.HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WordBase wordToInsert)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var word = _mapper.Map<Word>(wordToInsert);
            var result = await _wordService.UpsertWord(word);

            return this.HandleResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Word word)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _wordService.UpsertWord(word);

            return this.HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _wordService.DeleteWord(id);

            return this.HandleResult(result);
        }
    }
}
