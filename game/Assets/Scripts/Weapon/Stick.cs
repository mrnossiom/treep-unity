namespace Treep.Weapon {
    public class Stick : ICloseWeapon {
        public int Damage { get; }
        public int Durability { get; set; }
        public float AttackRange { get; }
        public float AttackRate { get; }

        public Stick() {
            this.Damage = 1;
            this.Durability = -1;
            this.AttackRange = 1.5f;
            this.AttackRate = 2f;
        }

        public override string ToString() {
            return "Name : stick\n" +
                   $"Damage : {this.Damage}\n" +
                   $"Durability : {this.Durability}\n" +
                   $"AttackRange : {this.AttackRange}\n" +
                   $"AttackRate : {this.AttackRate}\n";
        }
    }
}
