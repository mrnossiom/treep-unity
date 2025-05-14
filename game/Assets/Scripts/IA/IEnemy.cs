namespace Treep.IA {
    public interface IEnemy {
        public int PV { get; set; }

        public void Hit(int damageTook);
        public void Die();
    }
}
