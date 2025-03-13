namespace Treep.IA {
    public interface IEnemy {
        public int PV { get; set; }

        public void GetHitted(int damageTook);
        public void Die();
    }
}
