using CommonClass;
using System.Collections.Generic;
using UnityEngine;

public class NoteExplosionScript : MonoBehaviour
{
    public Animator ExpGood_pre;
    public Animator ExpGreat_pre;
    public Animator ExpBigGood_pre;
    public Animator ExpBigGreat_pre;
  

    public void Show(HitNoteResult state, NoteSoundScript note)
    {
        bool big = note.Type == 3 || note.Type == 4 || note.Type == 6;
        switch (state)
        {
            case HitNoteResult.Good:
                if (big)
                {
                    ExpBigGood_pre.SetTrigger("Show");
                    ExpBigGood_pre.transform.SetAsLastSibling();
                }
                else
                {
                    ExpGood_pre.SetTrigger("Show");
                    ExpGood_pre.transform.SetAsLastSibling();
                }
                break;
            case HitNoteResult.Perfect:
                if (big)
                {
                    ExpBigGreat_pre.SetTrigger("Show");
                    ExpBigGreat_pre.transform.SetAsLastSibling();
                }
                else
                {
                    ExpGreat_pre.SetTrigger("Show");
                    ExpGreat_pre.transform.SetAsLastSibling();
                }
                break;
        }
    }
    
}
