using UnityEngine;

public class TohoFireballScript : FireballScript
{
    public Transform Circle;

    private Vector3 position;
    private void OnEnable()
    {
        position = transform.localPosition;
        float x = Random.Range(-0.2f, 0.2f);
        transform.localPosition += new Vector3(x, 0);

        float angle = Random.Range(-35, 35);
        Circle.Rotate(0, 0, angle, Space.Self);
    }

    public override void End()
    {
        transform.localPosition = position;
        Circle.localRotation = Quaternion.identity;
        base.End();
    }
}
