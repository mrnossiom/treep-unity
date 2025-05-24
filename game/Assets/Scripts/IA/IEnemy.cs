namespace Treep.IA {
    public interface IEnemy {
        public float PV { get; set; }

        public int Hit(float damageTook);
        public void Die();
    }
}
