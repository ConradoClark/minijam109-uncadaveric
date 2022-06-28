using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TMP_Text LevelText;
    public int Level { get; private set; }

    private void Awake()
    {
        Level = 1;
        LevelText.text = "Level 1";
    }

    public void NextLevel()
    {
        Level++;
        LevelText.text = $"Level {Level}";
    }
}

