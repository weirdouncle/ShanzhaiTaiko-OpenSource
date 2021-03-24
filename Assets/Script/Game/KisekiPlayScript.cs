using UnityEngine;

public class KisekiPlayScript : MonoBehaviour
{
    public KisikiEffectScript[] Effects;

    public void ShowKisekiEffect(int index)
    {
        Effects[index].Play();
    }
}
