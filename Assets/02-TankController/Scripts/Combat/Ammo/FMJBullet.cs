namespace _02_TankController.Scripts.Combat.Ammo
{
    public class FMJBullet : BaseBullet 
    {
        public override void Awake()
        {
            base.Awake();
            m_Speed = 100f; // Faster
            m_Damage = 25f; // Hits harder
            m_Rb.useGravity = false; // "Flies straighter" (No arc)
            //todo implement an SO here to set the default values as opposed to hard coding them
        }
    }
}