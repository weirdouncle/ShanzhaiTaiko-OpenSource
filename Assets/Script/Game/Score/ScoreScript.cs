using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
    public GameObject AddAni_pre;
    public Transform AddParent;
    public SpriteNumberScript MainScore;
    public int Pools = 30;
    public AddScoreScript Bonus;
    public Animator Animator;

    public bool Counting
    {
        get => _counting;
        set
        {
            _counting = value;
            if (!_counting)
                ResetScore();
        }
    }

    private Queue<AddScoreScript> numbers = new Queue<AddScoreScript>();
    private List<AddScoreScript> all = new List<AddScoreScript>();
    private bool _counting;
    private Queue<int> scores = new Queue<int>();
    private Queue<int> main_scores = new Queue<int>();
    private float z_value = 0;
    private string scene;

    void Start()
    {
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (Bonus != null)
            Bonus.AddScoreEnd += OnAddEnd;

        StartCoroutine(Instantiate());
    }

    IEnumerator Instantiate()
    {
        int count = 0;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        yield return wait;

        for (int i = 0; i < Pools; i++)
        {
            if (count >= 20)
            {
                count = 0;
                yield return wait;
            }
            GameObject add = Instantiate(AddAni_pre, AddParent);
            AddScoreScript script = add.GetComponent<AddScoreScript>();
            numbers.Enqueue(script);

            script.AddScoreEnd += OnAddEnd;
            count++;
        }
    }

    void OnDestroy()
    {
        foreach (AddScoreScript script in all)
        {
            script.AddScoreEnd -= OnAddEnd;
            Destroy(script.gameObject);
        }

        if (Bonus != null)
            Bonus.AddScoreEnd -= OnAddEnd;
    }

    public void SetScore(int score)
    {
        /*
        if (numbers.Count > 0)
        {
            AddScoreScript script = numbers.Dequeue();
            script.transform.localPosition = new Vector3(0, 0, z_value);
            script.AddNumber(score);
            z_value -= 0.00001f;
        }
        else
        */
        scores.Enqueue(score);
        main_scores.Enqueue(score);
    }

    void Update()
    {
        if (scores.Count > 0 && numbers.Count > 0)
        {
            int score = scores.Dequeue();
            AddScoreScript script = numbers.Dequeue();
            script.transform.localPosition = new Vector3(0, 0, z_value);
            script.AddNumber(score);
            z_value -= 0.00001f;
        }

        if (main_scores.Count > 0 && (scene.Contains("Nijiiro") || scene.Contains("Dojo")))
        {
            int score = main_scores.Dequeue();
            MainScore.Count += score;
            Animator.SetTrigger("Add");
        }
    }

    public void AddBonusDiretly(int score)
    {
        MainScore.Count += score;
        Animator.SetTrigger("Add");
    }

    public void AddBonus(int score)
    {
        Bonus.AddNumber(score);
    }

    private void ResetScore()
    {
        MainScore.Count = 0;
    }

    private void OnAddEnd(AddScoreScript script, int score)
    {
        if (!script.Bonus) numbers.Enqueue(script);

        if (Counting && !scene.Contains("Nijiiro") && !scene.Contains("Dojo"))
        {
            MainScore.Count += score;
            Animator.SetTrigger("Add");
        }
    }
}
