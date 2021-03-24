using UnityEngine;

public class AvatarMaxScript : MonoBehaviour
{
    public SpriteRenderer[] Showings;

    private int shown_layer = 10;
    private int hidden_layer = 12;
    public void SetMax(bool max)
    {
        foreach (SpriteRenderer show in Showings)
            show.gameObject.layer = max ? shown_layer : hidden_layer; 
    }
}
