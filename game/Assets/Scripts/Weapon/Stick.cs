namespace Treep.Weapon
{
    public class Stick : IWeapon
    {
        public int Damage { get; }
        public int Durability { get; set; }
        public float AttackRange { get; }
        public float AttackRate { get; }

        public Stick()
        {
            Damage = 1;
            Durability = -1;
            AttackRange = 1.5f;
            AttackRate = 2f;
        }

        public override string ToString()
        {
            return "Name : stick\n" +
                   $"Damage : {Damage}\n" +
                   $"Durability : {Durability}\n" +
                   $"AttackRange : {AttackRange}\n" +
                   $"AttackRate : {AttackRate}\n";
        }
    }
}