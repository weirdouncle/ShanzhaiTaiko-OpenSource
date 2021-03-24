using CommonClass;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KusudamaDonControll : MonoBehaviour
{
    public GameObject Don;
    public GameObject Don2P;
    public Transform Don2PParent;
    public GloableAniControllerScript Global;
    public CriAtomSource Audio;
    public GameObject Kusudama_pre;
    public GameObject Hammer_pre;
    public Transform Parent;

    public UnityFx.Outline.OutlineEffect OutlineEffect;

    private Animator controll;
    private Animator don_Animator;
    private GameObject don;

    private Animator controll2;
    private Animator don2_Animator;
    private GameObject don2;
    //private KusudamaEndPlayScript script;
    public void OnInit()
    {
        don = Instantiate(Don, transform);
        Animator[] animators = don.GetComponentsInChildren<Animator>(true);
        controll = animators[0];
        don_Animator = animators[1];
        controll.keepAnimatorControllerStateOnDisable = false;

        GameObject pre = Hammer_pre;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro) pre = Kusudama_pre;
        GameObject game = Instantiate(pre, Parent);
        KusudamaShowScript show = game.GetComponent<KusudamaShowScript>();
        show.DonAnimator = this;
        show.Source = Audio;
        Global.Kusudama = new KusudamaShowScript[1] { show };

        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay || SceneManager.GetActiveScene().name.Contains("DualGame"))
        {
            don2 = Instantiate(Don2P, Don2PParent);
            Animator[] animators2 = don2.GetComponentsInChildren<Animator>(true);
            controll2 = animators2[0];
            don2_Animator = animators2[1];
            controll2.keepAnimatorControllerStateOnDisable = false;
        }

        if (OutlineEffect != null)
            OutlineEffect.UpdateOutlineObject();
    }

    public void OnStart()
    {
        don_Animator.gameObject.SetActive(true);
        don_Animator.transform.localRotation = Quaternion.AngleAxis(157, Vector3.up);
        controll.SetTrigger("Reset");

        if (don2_Animator != null)
        {
            don2_Animator.gameObject.SetActive(true);
            don2_Animator.transform.localRotation = Quaternion.AngleAxis(-157, Vector3.up);
            controll2.SetTrigger("Reset");
        }
    }

    public void Hit()
    {
        don_Animator.SetTrigger("Hit");
    }

    public void Hit2P()
    {
        don2_Animator.SetTrigger("Hit");
    }

    public void Fail()
    {
        don_Animator.SetTrigger("Fail");
        if (don2_Animator != null)
            don2_Animator.SetTrigger("Fail");
    }

    public void Success()
    {
        int index = Random.Range(0, 2);
        don_Animator.SetTrigger(index == 0 ? "Succe1" : "Succe2");
        controll.SetTrigger("Play");

        if (don2_Animator != null)
        {
            don2_Animator.SetTrigger(index == 0 ? "Succe1" : "Succe2");
            controll2.SetTrigger("Play");
        }
    }

    public void OnPlayEnd()
    {
        Global.ShowAvatar(false);
        Global.ShowAvatar2(false);
        don_Animator.gameObject.SetActive(false);
        if (don2_Animator != null)
        {
            don2_Animator.gameObject.SetActive(false);
        }
    }
}
