namespace Treep.Weapon {
    public interface IDistanceWeapon {
        public float Range { get; }
        public float Damage { get; }
        public int NbBounce { get; }
        public int MaxAmmunition { get; set; }
        public int Ammunition { get; }
    }
}
