using UnityEngine;

public class DamageTextContainer : Singleton<DamageTextContainer>
{
    public void AddDamageText(DamageText damageText)
    {
        damageText.transform.SetParent(transform);
    }
}
