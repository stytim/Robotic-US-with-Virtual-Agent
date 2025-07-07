// TextAnalyzer.cs
using System.Linq; // Optional, for more advanced queries if needed

public static class TextAnalyzer
{
    // --- Unicode Character Ranges ---

    // English Alphabet (Basic Latin)
    private const int ENG_UPPER_START = 0x0041; // 'A'
    private const int ENG_UPPER_END = 0x005A;   // 'Z'
    private const int ENG_LOWER_START = 0x0061; // 'a'
    private const int ENG_LOWER_END = 0x007A;   // 'z'

    // CJK Unified Ideographs (Common Chinese, Japanese Kanji, Korean Hanja)
    private const int CJK_UNIFIED_IDEOGRAPHS_START = 0x4E00;
    private const int CJK_UNIFIED_IDEOGRAPHS_END = 0x9FFF;

    // CJK Unified Ideographs Extension A
    private const int CJK_UNIFIED_IDEOGRAPHS_EXT_A_START = 0x3400;
    private const int CJK_UNIFIED_IDEOGRAPHS_EXT_A_END = 0x4DBF;

    // CJK Symbols and Punctuation (e.g., ¡££¬)
    private const int CJK_SYMBOLS_PUNCTUATION_START = 0x3000;
    private const int CJK_SYMBOLS_PUNCTUATION_END = 0x303F;

    // Add more ranges as needed (e.g., Hiragana, Katakana for Japanese, Hangul for Korean, other CJK extensions)
    // Hiragana (Japanese)
    // private const int HIRAGANA_START = 0x3040;
    // private const int HIRAGANA_END = 0x309F;
    // Katakana (Japanese)
    // private const int KATAKANA_START = 0x30A0;
    // private const int KATAKANA_END = 0x30FF;


    /// <summary>
    /// Helper method to check if a character is an English alphabet letter (A-Z, a-z).
    /// </summary>
    public static bool IsEnglishLetter(char c)
    {
        int unicodeVal = c;
        return (unicodeVal >= ENG_UPPER_START && unicodeVal <= ENG_UPPER_END) ||
               (unicodeVal >= ENG_LOWER_START && unicodeVal <= ENG_LOWER_END);
    }

    /// <summary>
    /// Helper method to check if a character is a CJK ideograph (commonly used for Chinese Hanzi).
    /// Note: This range also includes Japanese Kanji and Korean Hanja.
    /// </summary>
    public static bool IsCjkIdeograph(char c)
    {
        int unicodeVal = c;
        return (unicodeVal >= CJK_UNIFIED_IDEOGRAPHS_START && unicodeVal <= CJK_UNIFIED_IDEOGRAPHS_END) ||
               (unicodeVal >= CJK_UNIFIED_IDEOGRAPHS_EXT_A_START && unicodeVal <= CJK_UNIFIED_IDEOGRAPHS_EXT_A_END);
    }

    /// <summary>
    /// Helper method to check if a character is a CJK symbol or punctuation mark.
    /// </summary>
    public static bool IsCjkSymbolOrPunctuation(char c)
    {
        int unicodeVal = c;
        return (unicodeVal >= CJK_SYMBOLS_PUNCTUATION_START && unicodeVal <= CJK_SYMBOLS_PUNCTUATION_END);
    }


    /// <summary>
    /// Contains the result of a text analysis.
    /// </summary>
    public struct AnalysisResult
    {
        public int EnglishLetterCount { get; }
        public int CjkIdeographCount { get; } // Primarily Chinese Hanzi, Japanese Kanji, Korean Hanja
        public int DigitCount { get; }
        public int WhitespaceCount { get; }
        public int CjkPunctuationCount { get; }
        public int OtherPunctuationCount { get; }
        public int OtherCharCount { get; }
        public int TotalCharCount { get; }

        public AnalysisResult(int eng, int cjkIdeograph, int dig, int ws, int cjkPunc, int othPunc, int oth, int total)
        {
            EnglishLetterCount = eng;
            CjkIdeographCount = cjkIdeograph;
            DigitCount = dig;
            WhitespaceCount = ws;
            CjkPunctuationCount = cjkPunc;
            OtherPunctuationCount = othPunc;
            OtherCharCount = oth;
            TotalCharCount = total;
        }

        /// <summary>
        /// True if any English letters (A-Z, a-z) were found.
        /// </summary>
        public bool HasEnglishLetters => EnglishLetterCount > 0;

        /// <summary>
        /// True if any CJK ideographs (commonly Chinese characters) were found.
        /// </summary>
        public bool HasCjkIdeographs => CjkIdeographCount > 0;

        /// <summary>
        /// Calculates the proportion of a specific character count relative to the total characters.
        /// </summary>
        public float GetProportion(int count) => TotalCharCount == 0 ? 0 : (float)count / TotalCharCount;

        public override string ToString()
        {
            return $"Total: {TotalCharCount}, English: {EnglishLetterCount}, CJK Ideo: {CjkIdeographCount}, " +
                   $"Digits: {DigitCount}, Whitespace: {WhitespaceCount}, CJK Punct: {CjkPunctuationCount}, Other Punct: {OtherPunctuationCount}, Other: {OtherCharCount}";
        }
    }

    /// <summary>
    /// Analyzes the input string and counts different types of characters.
    /// </summary>
    /// <param name="text">The string to analyze.</param>
    /// <returns>An AnalysisResult struct containing character counts.</returns>
    public static AnalysisResult Analyze(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new AnalysisResult(0, 0, 0, 0, 0, 0, 0, 0);
        }

        int engCount = 0;
        int cjkIdeographCount = 0;
        int digitCount = 0;
        int whitespaceCount = 0;
        int cjkPunctuationCount = 0;
        int otherPunctuationCount = 0;
        int otherCount = 0;

        foreach (char c in text)
        {
            if (IsEnglishLetter(c)) { engCount++; }
            else if (IsCjkIdeograph(c)) { cjkIdeographCount++; }
            // Order matters if ranges overlap or if a char can be multiple things
            // For CJK Punctuation vs standard char.IsPunctuation:
            else if (IsCjkSymbolOrPunctuation(c)) { cjkPunctuationCount++; }
            else if (char.IsDigit(c)) { digitCount++; }
            else if (char.IsWhiteSpace(c)) { whitespaceCount++; }
            else if (char.IsPunctuation(c)) { otherPunctuationCount++; } // Catches standard punctuation not covered by CJK
            else { otherCount++; }
        }

        return new AnalysisResult(
            engCount,
            cjkIdeographCount,
            digitCount,
            whitespaceCount,
            cjkPunctuationCount,
            otherPunctuationCount,
            otherCount,
            text.Length
        );
    }
}