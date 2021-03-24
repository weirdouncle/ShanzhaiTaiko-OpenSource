using System.Collections.Generic;
using UnityEngine;

public class RendaControllerScript : MonoBehaviour
{
    public GameObject[] Renda_pres;

    private RendaScript renda;

    void Start()
    {
        LoadAsset();
    }

    private void LoadAsset()
    {
        GameObject game = Instantiate(Renda_pres[0], transform);
        renda = game.GetComponent<RendaScript>();
    }

    public void DoRapitHit()
    {
        renda.OnPlay();
    }
}
