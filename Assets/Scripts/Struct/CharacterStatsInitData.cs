namespace Struct
{
    public struct CharacterStatsInitData
    {
        public float baseHealth;
        public float baseShield;
        public float baseAttack;
        public float baseDefense;
        public float baseSpeed;

        public static CharacterStatsInitData Default => new CharacterStatsInitData
        {
            baseHealth = 100f,
            baseShield = 0f,
            baseAttack = 10f,
            baseDefense = 5f,
            baseSpeed = 5f
        };
    }
}