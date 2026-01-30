# EasyXR Interaction Framework

A lightweight, event‑driven VR interaction system for Unity.  
Built for speed, clarity, and modularity, no Update loops, no heavy physics, just clean XR interactions.


## Features 
- Event‑based XR interactions (no Update loops)
- Hand, body, or combined activation modes
- Built‑in cooldown system
- Custom editor with foldouts and debug tools
- Works with Gorilla‑Tag‑style physics or more
- Easy to extend for teleporters, UI, shops, and more

## Installation Process
- Download the `EasyXR` package
- Drag it into your Unity project
- Add an `XRButton` or any subclass to your scene

## Quick-start example

Add an `XRButton` to any object with a collider.  
Then extend it:

```csharp
using EasyXRSystems;

public class DoorButton : XRButton
{
    public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
    {
        if (isPressed)
            OpenDoor();
    }

    private void OpenDoor()
    {
        Debug.Log("Door opened!");
    }
}
```
## Coding-snippets


Handling Both Hand + Body Presses

```csharp
public override void ButtonActivationWithBody(bool isPressed)
    => HandlePress(isPressed, TypesofTouch.Body);

public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
    => HandlePress(isPressed, TypesofTouch.Hand);
 ```

Teleporter example

```csharp
public class EasyXRTeleporter : XRButton
{
    public Transform destination;

    public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
    {
        if (isPressed)
            TeleportPlayer(destination.position);
    }
}
```

## Built-in editor tools

- Foldout sections for clean organization  
- Debug Tools that only activate in Play Mode  
- Auto‑disabled UI in Edit Mode  
- Helpful warnings for missing references

## FAQ

**Does EasyXR use Update()?**  
No — everything is event‑driven for maximum performance.

**Does it work with Gorilla‑Tag‑style physics?**  
Yes. XRButtons were designed with physics‑based VR locomotion in mind.

