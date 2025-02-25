using System;
using System.Collections;
using Mirror;
using Treep.Weapon;
using UnityEngine;
namespace Treep.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        
        // Attack
        public Transform attackPointRight;
        public Transform attackPointLeft;
        public Transform attackPointTop;
        public Transform attackPointBottom;
        public Vector2 attackPoint;
        
        public LayerMask ennemyLayerMask;
        
        private float nextAttackTime = 0;
        private Controller ContollerScript;
        private Looking currentLooking;

        public int PVMax = 10;
        private int PV { get; set; }

        private IWeapon _currentWeapon;
        

        public void Awake()
        {
            ContollerScript = GetComponent<Controller>();
            currentLooking = ContollerScript.looking;
            PV = PVMax;
            _currentWeapon = new Stick();
        }
        

        public void Update()
        {
            UpdateAttackPoint();
            if (Time.time >= nextAttackTime)
            {
                
                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Attack();
                    nextAttackTime = Time.time + 1f / _currentWeapon.AttackRate;
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log($"{_currentWeapon}");
                Debug.Log($"{PV}");
                Debug.Log(attackPoint);

            }
        }
        

        private void UpdateAttackPoint()
        {
            currentLooking = ContollerScript.looking;
            switch (currentLooking)
            {
                case Looking.Right :
                    attackPoint = attackPointRight.position;
                    break;
                case Looking.Left :
                    attackPoint = attackPointLeft.position;
                    break;
                case Looking.Top :
                    attackPoint = attackPointTop.position;
                    break;
                case Looking.Bottom :
                    attackPoint = attackPointBottom.position;
                    break;
                    
            }
           
        }
        
        private void Attack()
        {
            Debug.Log($"Player attack");
            // animation
            //_animator.SetTrigger("Attack");
            
            //check enemy
            Collider2D[] hitEnnemys =  Physics2D.OverlapCircleAll(attackPoint, _currentWeapon.AttackRange, ennemyLayerMask);
            //Damage to ennemy
            foreach (var ennemy in hitEnnemys)
            {
                Debug.Log($"{ennemy.name} took {_currentWeapon.Damage} damage");
                ennemy.GetComponent<IEnemy>().GetHitted(_currentWeapon.Damage);
            }
        }
        
        public void OnDrawGizmosSelected()
        {
            if (attackPointRight == null 
                || attackPointLeft == null
                || attackPointTop == null
                || attackPointBottom == null)
                return;
            
            //Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
            //Gizmos.DrawWireSphere(attackPointLeft.position, attackRange);
            //Gizmos.DrawWireSphere(attackPointTop.position, attackRange);
            //Gizmos.DrawWireSphere(attackPointBottom.position, attackRange);
            Gizmos.DrawWireSphere(attackPoint, _currentWeapon.AttackRange);
            
        }
    }
}