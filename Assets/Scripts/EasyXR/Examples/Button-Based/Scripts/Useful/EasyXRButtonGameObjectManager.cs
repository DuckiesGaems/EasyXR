using UnityEngine;
using EasyXRSystems;

public class EasyXRButtonGameObjectManager : XRButton
{
    public enum GameObjectManagerEnum{Enable, Disable}

    public GameObjectManagerEnum GameModeActiveState = GameObjectManagerEnum.Disable;
public GameObject[] EffectedObjects;
private GameObject ThisGameobject;
public bool IsToggle;
private bool ActiveState => GameModeActiveState == GameObjectManagerEnum.Enable;

private bool ToggleState = false; // false = enabled, true = disabled

    protected override void Start()
    {
        base.Start();
        ThisGameobject = this.gameObject;
    }

public override void ButtonActivationWithBody(bool isPressed)
    => HandleButtonpress(isPressed);
public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
    => HandleButtonpress(isPressed);

    public void HandleButtonpress(bool isPressed)
    {
if (EffectedObjects.Length > 0)
        {
     if (IsToggle)
            {
                if (isPressed)
                {
                    foreach (GameObject Obj in EffectedObjects)
                    {
                        if (Obj == ThisGameobject) continue;
                     Obj.SetActive(!ToggleState);
                    }
                    ToggleState = !ToggleState;
                }
            }
            else
                            if (isPressed)
                {
                    foreach (GameObject Obj in EffectedObjects)
                    {
                    if (Obj == ThisGameobject) continue;
                     Obj.SetActive(ActiveState);
                    }
                }
        }
        else
        {
            Debug.LogError("No objects found in DisablingObjects on gameobject " + ThisGameobject);
        }
    }
}
