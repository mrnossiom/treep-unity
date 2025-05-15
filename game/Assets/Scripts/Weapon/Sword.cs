namespace Treep.Weapon {
    public class Sword : CircleHitboxCloseWeapon {
        private void Awake() {
            this.Name = "Sword";
            this.Damage = 4;
            this.AttackRate = 3f;
            this.BaseAwake(2f);
        }


        public override string ToString() {
            return $"Name : {this.Name}\n" +
                   $"Damage : {this.Damage}\n" +
                   $"AttackRate : {this.AttackRate}\n";
        }
    }
}
