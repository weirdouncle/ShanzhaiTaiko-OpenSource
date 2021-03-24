using System.Collections.Generic;
using UnityEngine;

public class UpDuelFrameScript : UpFrameControllerScript
{
    public GameObject[] Move2_pres;
    public Transform Player2Panel;

    private UpMoveScript move2;
    void Start()
    {
        GameObject game = Instantiate(Move_pres[0], transform);
        move = game.GetComponent<UpMoveScript>();

        GameObject game2 = Instantiate(Move2_pres[0], Player2Panel);
        move2 = game2.GetComponent<UpMoveScript>();
    }

    public override void SetClear2P(bool clear)
    {
        move2.Clear(clear);
    }
}
