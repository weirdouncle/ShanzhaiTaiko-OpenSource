using UnityEngine;

public class NoteTickScript : MonoBehaviour
{
    public Animator Animator;
    public bool Player2;
    private NoteSoundScript.ComboType combo;

    public static int ComboType;
    public static int ComboType2P;
    public static int ComboTypeBig;
    public static int ComboTypeBig2P;

    private void Start()
    {
        if (Player2)
            ComboType2P = ComboTypeBig2P = 0;
        else
            ComboType = ComboTypeBig = 0;
    }

    public void SetCombo(NoteSoundScript.ComboType combo)
    {
        if (this.combo != combo)
        {
            this.combo = combo;
            if (combo == NoteSoundScript.ComboType.Combo_None)
            {
                Animator.enabled = false;
                if (Player2)
                    ComboType2P = ComboTypeBig2P = 0;
                else
                    ComboType = ComboTypeBig = 0;
            }
            else
            {
                if (!Animator.enabled) Animator.enabled = true;
                switch (combo)
                {
                    case NoteSoundScript.ComboType.Combo_50:
                        Animator.SetTrigger("50");
                        break;
                    case NoteSoundScript.ComboType.Combo_150:
                        Animator.SetTrigger("150");
                        break;
                    case NoteSoundScript.ComboType.Combo_300:
                        Animator.SetTrigger("300");
                        break;
                }
            }
        }
    }

    public void SendNoteTick(int type)
    {
        int count = type > 0 ? type - 1 : type;
        int big_count = type > 1 ? 0 : type;

        if (Player2)
        {
            ComboType2P = count;
            ComboTypeBig2P = big_count;
        }
        else
        {
            ComboType = count;
            ComboTypeBig = big_count;
        }
    }
}
