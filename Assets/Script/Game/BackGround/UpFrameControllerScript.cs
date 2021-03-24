using System.Collections.Generic;
using UnityEngine;

public class UpFrameControllerScript : MonoBehaviour
{
    public GameObject[] Move_pres;

    protected UpMoveScript move;
    void Start()
    {
        GameObject game = Instantiate(Move_pres[0], transform);
        move = game.GetComponent<UpMoveScript>();
    }

    public void SetClear(bool clear)
    {
        if (move != null) move.Clear(clear);
    }

    public virtual void SetClear2P(bool clear)
    {
    }
}
