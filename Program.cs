using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.Console;
//args = new[] { "text.txt" };

if (args.Length < 1)
{
    throw new ArgumentException("Filename not provided.");
}
string[] ignoredWords = new[] { "in", "of", "a", "the", "at", "through", "up", "down", "or", "and", "no", "not", "to", "on", "above", "below" };
string text = null!;
using (StreamReader reader = File.OpenText(args[0]))
{
    text = reader.ReadToEnd();
}

int curLine = 1;
List<(string word, int line)> words = new();
string curWord = "";
int textPos = 0;
traverseTextLoop:
char curChar = text[textPos];
// ignore punctuation
if (char.IsWhiteSpace(curChar) || curChar == ','
|| curChar == '.' || curChar == ':' || curChar == ';' || curChar == '!'
|| curChar == '?' || curChar == ')' || curChar == '(' || curChar == '"')
{
    // word input over
    if (!string.IsNullOrWhiteSpace(curWord))
    {
        words.Add((curWord, curLine));
    }
    curWord = "";
    // increase line counter
    if (curChar == '\n')
    {
        curLine++;
    }
}
else
{
    curWord += char.ToLower(curChar);
}
textPos++;
if (textPos < text.Length)
{
    goto traverseTextLoop;
}

// flush remaining word if exists
if (!string.IsNullOrWhiteSpace(curWord))
{
    words.Add((curWord, curLine));
}

// filter out ignored words
int wordsPos = 0;
int ignoredPos = 0;
traverseWordsLoop:
// check if word is in ignored list
ignoredPos = 0;
traverseIgnoredLoop:
if (ignoredWords[ignoredPos] == words[wordsPos].word)
{
    words.RemoveAt(wordsPos);
    wordsPos--;
    goto endTraverseIgnoredLoop;
}
ignoredPos++;
if (ignoredPos < ignoredWords.Length)
{
    goto traverseIgnoredLoop;
}
endTraverseIgnoredLoop:

wordsPos++;
if (wordsPos < words.Count)
{
    goto traverseWordsLoop;
}

const int linesPerPage = 45;

// count occurences
Dictionary<string, (int count, List<int> pages)> wordInfo = new();
int curWordInd = 0;
countOccurencesLoop:
string curCountingWord = words[curWordInd].word;
// ContainsKey shouldn't loop internally, so it's ok
int page = (words[curWordInd].line - 1) / linesPerPage + 1;
if (wordInfo.ContainsKey(curCountingWord))
{
    wordInfo[curCountingWord] = (wordInfo[curCountingWord].count + 1, wordInfo[curCountingWord].pages);
    // add page if it hasn't been added already
    if (wordInfo[curCountingWord].pages[^1] != page)
    {
        wordInfo[curCountingWord].pages.Add(page);
    }
    words.RemoveAt(curWordInd);
    curWordInd--;
}
else
{
    wordInfo[curCountingWord] = (1, new List<int>() { page });
}
curWordInd++;
if (curWordInd < words.Count)
{
    goto countOccurencesLoop;
}

// sort words by their frequency
int i = 0;
int j = 0;
outerBubbleLoop:
j = i + 1;
innerBubbleLoop:
if (wordInfo[words[j].word].count > wordInfo[words[j - 1].word].count)
{
    (words[j], words[j - 1]) = (words[j - 1], words[j]);
}
j++;
if (j < words.Count)
{
    goto innerBubbleLoop;
}

i++;
if (i < words.Count - 1)
{
    goto outerBubbleLoop;
}

// output frequency
curWordInd = 0;
outputWord:
WriteLine($"{words[curWordInd].word} - {wordInfo[words[curWordInd].word].count}");
curWordInd++;
if (curWordInd < words.Count)
{
    goto outputWord;
}

// output index
WriteLine(@"-----------------------------------
INDEX
--------------------------------");

// sort alphabetically
// sort words by their frequency
i = 0;
j = 0;
outerBubbleLoopAlpha:
j = i + 1;
innerBubbleLoopAlpha:
if (string.Compare(words[j].word, words[j - 1].word) < 0)
{
    (words[j], words[j - 1]) = (words[j - 1], words[j]);
}
j++;
if (j < words.Count)
{
    goto innerBubbleLoopAlpha;
}

i++;
if (i < words.Count - 1)
{
    goto outerBubbleLoopAlpha;
}

const int maxFreq = 100;
curWordInd = 0;
outputIndex:
// skip if more than 100 times
if (wordInfo[words[curWordInd].word].count > maxFreq) {
    goto skip;
}
Write($"{words[curWordInd].word} - ");
int curPageInd = 0;
outputPages:
Write($"{wordInfo[words[curWordInd].word].pages[curPageInd]} ");
curPageInd++;
if (curPageInd < wordInfo[words[curWordInd].word].pages.Count) {
    goto outputPages;
}
WriteLine();
skip:
curWordInd++;
if (curWordInd < words.Count)
{
    goto outputIndex;
}
