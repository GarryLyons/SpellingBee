using Backend.Services;

namespace Backend.Endpoints;

public static class WordsEndpoints
{
    public static RouteGroupBuilder MapWordsEndpoints(this RouteGroupBuilder apiGroup)
    {
        var wordsGroup = apiGroup.MapGroup("/words").WithTags("Words").RequireAuthorization();

        wordsGroup.MapGet("/", (WordBankRepository wordBankRepository, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("WordsEndpoints");
            var words = wordBankRepository.GetAll();
            logger.LogInformation("Returning {WordCount} words from in-memory word bank.", words.Count);
            return Results.Ok(words);
        });

        return wordsGroup;
    }
}
