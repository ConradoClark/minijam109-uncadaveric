using Licht.Unity.Objects;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChallengeComponents : BaseUIObject
{
    public TMP_Text ActivityText;

    public void SetActivity(string activity, Color color)
    {
        ActivityText.text = $"Activity: <color=#{color.ToHexString()}> {activity}";
    }
}
