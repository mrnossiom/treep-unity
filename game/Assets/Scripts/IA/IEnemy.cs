namespace Treep.IA {
    public interface IEnemy {
        public float PV { get; set; }

        public void Hit(float damageTook);
        public void Die();
    }
}
