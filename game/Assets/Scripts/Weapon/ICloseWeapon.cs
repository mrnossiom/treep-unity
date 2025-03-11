namespace Treep.Weapon {
    public interface ICloseWeapon {
        public int Damage { get; }
        public int Durability { get; set; }
        public float AttackRange { get; }

        /// <summary>
        /// How many times we can hit per second
        /// </summary>
        public float AttackRate { get; }
    }
}
