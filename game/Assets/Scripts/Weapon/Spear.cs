namespace Treep.Weapon {
    public class Spear : RectHitboxCloseWeapon {
        public int Durability { get; set; }

        private void Awake() {
            this.Name = "Spear";
            this.Damage = 1;
            this.Durability = 1;
            this.AttackRate = 2f;

            // Modify HitBox of the Weapon
            var top = (1, 4);
            var right = (4, 1);

            this.BaseAwake(top, right);
        }
    }
}
