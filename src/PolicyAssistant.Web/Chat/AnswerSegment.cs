using System.Text.RegularExpressions;
using PolicyAssistant.Shared;

namespace PolicyAssistant.Web.Chat;

/// <summary>A piece of an answer paragraph: plain text or a citation badge.</summary>
public abstract record AnswerSegment;

public sealed record TextSegment(string Text) : AnswerSegment;

/// <summary>One or more adjacent citations, e.g. from [1][2] or [1, 2], rendered as a group of badges.</summary>
public sealed record CitationSegment(IReadOnlyList<Citation> Citations) : AnswerSegment;

public static partial class AnswerParser
{
    /// <summary>
    /// Splits an answer into paragraphs (one per non-empty line) of text and citation segments.
    /// Markers like [1] or [1, 3] become badges for the numbers present in <paramref name="citations"/>;
    /// markers with no matching citation are left as text.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<AnswerSegment>> Parse(string answer, IReadOnlyList<Citation> citations)
    {
        var byNumber = citations.DistinctBy(c => c.Number).ToDictionary(c => c.Number);

        return answer.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Select(line => ParseLine(line, byNumber))
            .ToList();
    }

    private static IReadOnlyList<AnswerSegment> ParseLine(string line, Dictionary<int, Citation> byNumber)
    {
        var segments = new List<AnswerSegment>();
        var position = 0;

        foreach (Match match in CitationMarker().Matches(line))
        {
            var cited = match.Groups[1].Value.Split(',')
                .Select(number => int.Parse(number.Trim()))
                .Where(byNumber.ContainsKey)
                .Distinct()
                .ToList();
            if (cited.Count == 0)
            {
                continue;
            }

            var citationsHere = cited.Select(number => byNumber[number]);
            if (match.Index > position)
            {
                segments.Add(new TextSegment(line[position..match.Index]));
                segments.Add(new CitationSegment(citationsHere.ToList()));
            }
            else if (segments.LastOrDefault() is CitationSegment previous)
            {
                // Directly follows another marker: merge so the badges stay together.
                segments[^1] = new CitationSegment(previous.Citations.Concat(citationsHere).DistinctBy(c => c.Number).ToList());
            }
            else
            {
                segments.Add(new CitationSegment(citationsHere.ToList()));
            }

            position = match.Index + match.Length;
        }

        if (position < line.Length)
        {
            segments.Add(new TextSegment(line[position..]));
        }

        return segments;
    }

    [GeneratedRegex(@"\[(\d{1,3}(?:\s*,\s*\d{1,3})*)\]")]
    private static partial Regex CitationMarker();
}
