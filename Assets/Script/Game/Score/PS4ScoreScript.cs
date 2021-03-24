using UnityEngine;

public class PS4ScoreScript : AddScoreScript
{
    public Transform Plus;
    protected override void UpdateCount()
    {
        int Count = _count;
        if (count == Count) return;
        string str = Reverse1(Count.ToString());

        int position = 0;
        for (int i = 0; i < Images.Length; i++)
        {
            bool activa = str.Length >= i + 1;
            Images[i].gameObject.SetActive(activa);
            if (activa)
            {
                Images[i].sprite = Sprites[int.Parse(str[i].ToString())];
                position = i + 1;
            }
        }
        count = Count;
        Plus.localPosition = new Vector3(-0.2f * position, 0);
        Plus.gameObject.SetActive(true);
    }

    public override void End()
    {
        base.End();
        Plus.gameObject.SetActive(false);
    }
}
