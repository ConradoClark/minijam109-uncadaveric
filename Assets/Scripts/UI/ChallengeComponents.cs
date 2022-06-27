using Licht.Unity.Objects;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChallengeComponents : BaseUIObject
{
    public Vector3 VirusSpawnLeft;
    public Vector3 VirusSpawnRight;
    public TMP_Text ActivityText;

    public void SetActivity(string activity, Color color)
    {
        ActivityText.text = $"Activity: <color=#{color.ToHexString()}><size=4>{activity}";
    }
}
