namespace Treep.Weapon
{
    public interface IWeapon
    {
        public int Damage { get; }
        public int Durability { get; set; }
        public float AttackRange { get; }
        public float AttackRate { get; } // combien de fois on peu taper par seconde
    }
}