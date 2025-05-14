namespace Treep.Weapon {
    public interface ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }

        /// <summary>
        /// How many times we can hit per second
        /// </summary>
        public float AttackRate { get; set; }

        public WeaponHitbox Hitbox { get; set; }
    }
}
