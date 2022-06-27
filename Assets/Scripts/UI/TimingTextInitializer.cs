using Licht.Unity.Objects;
using UnityEngine;

public class TimingTextInitializer : MonoBehaviour
{
    public ScriptPrefab TimingText;
    private void Start()
    {
        var manager = SceneObject<TimingTextManager>.Instance();
        var eff = manager.GetEffect(TimingText);
        eff.Activate();
        if (eff.TryGetFromPool(out var obj))
        {
            obj.TextComponent.text = "";
        }
    }

}
