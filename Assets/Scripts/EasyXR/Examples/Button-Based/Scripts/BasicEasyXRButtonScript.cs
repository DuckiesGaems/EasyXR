using UnityEngine;
using EasyXRSystems;

public class BasicEasyXRButtonScript : XRButton
{
    public Color PressedColor = Color.green;
    private Color UnpressedColor;
    private Material instanceMat;

    protected override void Awake()
    {
        base.Awake();



        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            instanceMat = rend.material;
            UnpressedColor = instanceMat.color;
        }
    }

    public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
    {
        if (instanceMat == null) return;

        if (isPressed)
        {
            instanceMat.color = PressedColor;
        }
        else
        {
            instanceMat.color = UnpressedColor;
           
        }
    }
}
