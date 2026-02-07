using Backend.Models;

namespace Backend.Services;

public sealed class WordBankRepository
{
    private readonly List<WordEntry> _words =
    [
        new()
        {
            Id = "cat",
            Word = "cat",
            Segmentation = new SegmentationOptions { OnsetRime = "c | at" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/k/", "/a/", "/t/"],
                Graphemes = ["c", "a", "t"],
                Segmenting = "c-a-t",
                Blending = "Blend /k/ + /a/ + /t/ to read cat.",
                Digraphs = [],
                SplitDigraphs = [],
                Rules = ["Short vowel: a says /a/ in cat."]
            }
        },
        new()
        {
            Id = "ship",
            Word = "ship",
            Segmentation = new SegmentationOptions { OnsetRime = "sh | ip" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/sh/", "/i/", "/p/"],
                Graphemes = ["sh", "i", "p"],
                Segmenting = "sh-i-p",
                Blending = "Blend /sh/ + /i/ + /p/ to read ship.",
                Digraphs = ["sh"],
                SplitDigraphs = [],
                Rules = ["sh is a digraph: two letters making one sound."]
            }
        },
        new()
        {
            Id = "cake",
            Word = "cake",
            Segmentation = new SegmentationOptions { OnsetRime = "c | ake" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/k/", "/ay/", "/k/"],
                Graphemes = ["c", "a_e", "k"],
                Segmenting = "c-a(k)-e",
                Blending = "Blend /k/ + /ay/ + /k/ to read cake.",
                Digraphs = [],
                SplitDigraphs = ["a_e"],
                Rules = ["Split digraph a_e makes the long /ay/ sound."]
            }
        },
        new()
        {
            Id = "light",
            Word = "light",
            Segmentation = new SegmentationOptions { OnsetRime = "l | ight" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/l/", "/igh/", "/t/"],
                Graphemes = ["l", "igh", "t"],
                Segmenting = "l-igh-t",
                Blending = "Blend /l/ + /igh/ + /t/ to read light.",
                Digraphs = ["gh"],
                SplitDigraphs = [],
                Rules = ["igh is a trigraph that usually says /igh/."]
            }
        },
        new()
        {
            Id = "spoon",
            Word = "spoon",
            Segmentation = new SegmentationOptions { OnsetRime = "sp | oon" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/s/", "/p/", "/oo/", "/n/"],
                Graphemes = ["s", "p", "oo", "n"],
                Segmenting = "s-p-oo-n",
                Blending = "Blend /s/ + /p/ + /oo/ + /n/ to read spoon.",
                Digraphs = ["oo"],
                SplitDigraphs = [],
                Rules = ["oo often makes the long /oo/ sound."]
            }
        },
        new()
        {
            Id = "train",
            Word = "train",
            Segmentation = new SegmentationOptions { OnsetRime = "tr | ain" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/t/", "/r/", "/ay/", "/n/"],
                Graphemes = ["t", "r", "ai", "n"],
                Segmenting = "t-r-ai-n",
                Blending = "Blend /t/ + /r/ + /ay/ + /n/ to read train.",
                Digraphs = ["ai"],
                SplitDigraphs = [],
                Rules = ["ai usually spells the long /ay/ sound in the middle of a word."]
            }
        },
        new()
        {
            Id = "jump",
            Word = "jump",
            Segmentation = new SegmentationOptions { OnsetRime = "j | ump" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/j/", "/u/", "/m/", "/p/"],
                Graphemes = ["j", "u", "m", "p"],
                Segmenting = "j-u-m-p",
                Blending = "Blend /j/ + /u/ + /m/ + /p/ to read jump.",
                Digraphs = [],
                SplitDigraphs = [],
                Rules = ["Closed syllable: u says /u/ in jump."]
            }
        },
        new()
        {
            Id = "phone",
            Word = "phone",
            Segmentation = new SegmentationOptions { OnsetRime = "ph | one" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/f/", "/oh/", "/n/"],
                Graphemes = ["ph", "o_e", "n"],
                Segmenting = "ph-o(n)-e",
                Blending = "Blend /f/ + /oh/ + /n/ to read phone.",
                Digraphs = ["ph"],
                SplitDigraphs = ["o_e"],
                Rules = ["ph can represent the /f/ sound.", "Split digraph o_e makes /oh/."]
            }
        },
        new()
        {
            Id = "kite",
            Word = "kite",
            Segmentation = new SegmentationOptions { OnsetRime = "k | ite" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/k/", "/igh/", "/t/"],
                Graphemes = ["k", "i_e", "t"],
                Segmenting = "k-i(t)-e",
                Blending = "Blend /k/ + /igh/ + /t/ to read kite.",
                Digraphs = [],
                SplitDigraphs = ["i_e"],
                Rules = ["Split digraph i_e makes the long /igh/ sound."]
            }
        },
        new()
        {
            Id = "chair",
            Word = "chair",
            Segmentation = new SegmentationOptions { OnsetRime = "ch | air" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/ch/", "/air/"],
                Graphemes = ["ch", "air"],
                Segmenting = "ch-air",
                Blending = "Blend /ch/ + /air/ to read chair.",
                Digraphs = ["ch"],
                SplitDigraphs = [],
                Rules = ["ch is a digraph.", "air is a vowel team that says /air/."]
            }
        },
        new()
        {
            Id = "seed",
            Word = "seed",
            Segmentation = new SegmentationOptions { OnsetRime = "s | eed" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/s/", "/ee/", "/d/"],
                Graphemes = ["s", "ee", "d"],
                Segmenting = "s-ee-d",
                Blending = "Blend /s/ + /ee/ + /d/ to read seed.",
                Digraphs = ["ee"],
                SplitDigraphs = [],
                Rules = ["ee is a vowel digraph that says /ee/."]
            }
        },
        new()
        {
            Id = "rope",
            Word = "rope",
            Segmentation = new SegmentationOptions { OnsetRime = "r | ope" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/r/", "/oh/", "/p/"],
                Graphemes = ["r", "o_e", "p"],
                Segmenting = "r-o(p)-e",
                Blending = "Blend /r/ + /oh/ + /p/ to read rope.",
                Digraphs = [],
                SplitDigraphs = ["o_e"],
                Rules = ["Split digraph o_e makes the long /oh/ sound."]
            }
        },
        new()
        {
            Id = "cloud",
            Word = "cloud",
            Segmentation = new SegmentationOptions { OnsetRime = "cl | oud" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/k/", "/l/", "/ow/", "/d/"],
                Graphemes = ["c", "l", "ou", "d"],
                Segmenting = "c-l-ou-d",
                Blending = "Blend /k/ + /l/ + /ow/ + /d/ to read cloud.",
                Digraphs = ["ou"],
                SplitDigraphs = [],
                Rules = ["ou can represent the /ow/ sound."]
            }
        },
        new()
        {
            Id = "brave",
            Word = "brave",
            Segmentation = new SegmentationOptions { OnsetRime = "br | ave" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/b/", "/r/", "/ay/", "/v/"],
                Graphemes = ["b", "r", "a_e", "v"],
                Segmenting = "b-r-a(v)-e",
                Blending = "Blend /b/ + /r/ + /ay/ + /v/ to read brave.",
                Digraphs = [],
                SplitDigraphs = ["a_e"],
                Rules = ["Split digraph a_e makes the long /ay/ sound."]
            }
        },
        new()
        {
            Id = "cheese",
            Word = "cheese",
            Segmentation = new SegmentationOptions { OnsetRime = "ch | eese" },
            Phonics = new PhonicsBreakdown
            {
                Phonemes = ["/ch/", "/ee/", "/z/"],
                Graphemes = ["ch", "ee", "se"],
                Segmenting = "ch-ee-se",
                Blending = "Blend /ch/ + /ee/ + /z/ to read cheese.",
                Digraphs = ["ch", "ee"],
                SplitDigraphs = [],
                Rules = ["Final se often says /z/ in words like cheese."]
            }
        }
    ];

    private readonly Dictionary<string, WordEntry> _wordsById;

    public WordBankRepository()
    {
        _wordsById = _words.ToDictionary(word => word.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<WordEntry> GetAll()
    {
        return _words;
    }

    public IReadOnlyList<WordEntry> GetByIds(IReadOnlyList<string> ids)
    {
        var selected = new List<WordEntry>(ids.Count);
        foreach (var id in ids)
        {
            if (_wordsById.TryGetValue(id, out var word))
            {
                selected.Add(word);
            }
        }

        return selected;
    }

    public bool ContainsAll(IReadOnlyList<string> ids, out List<string> missingIds)
    {
        missingIds = ids
            .Where(id => !_wordsById.ContainsKey(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return missingIds.Count == 0;
    }
}
