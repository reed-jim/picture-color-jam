using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    public float attackDamage => GamePersistentVariable.mainCharacterStat.damage;
}
