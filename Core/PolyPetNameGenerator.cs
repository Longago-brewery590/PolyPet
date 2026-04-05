using System;

namespace PolyPet
{
    public static class PolyPetNameGenerator
    {
        private static readonly string[] Syllables =
        {
            "mo", "chi", "pi", "ku", "zu", "bi", "na", "ri",
            "lo", "ta", "po", "nu", "ki", "ze", "ba", "mi",
            "do", "fu", "go", "li", "wa", "shi", "ra", "ne",
            "yo", "bu", "ji", "pa", "so", "tu"
        };

        public static string Create(int seed)
        {
            var rng = new Random(seed);
            int syllableCount = rng.Next(2, 4); // 2 or 3 syllables
            var name = new char[syllableCount * 3]; // max 3 chars per syllable
            int pos = 0;

            for (int i = 0; i < syllableCount; i++)
            {
                var syllable = Syllables[rng.Next(Syllables.Length)];
                foreach (char c in syllable)
                {
                    name[pos++] = c;
                }
            }

            name[0] = char.ToUpper(name[0]);
            return new string(name, 0, pos);
        }
    }
}
