# EasyXR Interaction Framework

A lightweight, event‑driven VR interaction system for Unity.  
Built for speed, clarity, and modularity, no Update loops, no heavy physics, just clean XR interactions.

## Table of contents

[XRButton](#xrbutton)  
[XREasyGrab](#xr-easy-grab)  



## XRButton



### Features 
- Event‑based XR interactions (no Update loops)
- Hand, body, or combined activation modes
- Built‑in cooldown system
- Custom editor with foldouts and debug tools
- Works with Gorilla‑Tag‑style physics and similar movement systems
- Easy to extend for teleporters, UI, shops, and more

<b>Example Script</b>
> [!NOTE]
> This example script is included in the Unity package.

<img width="697" height="364" alt="image" src="https://github.com/user-attachments/assets/e0842bee-4a25-4a38-80da-322ed99dc982" />

### Installation Process
- Download the `EasyXR` package
- Drag it into your Unity project
- Add an `XRButton` or any subclass to your scene

### Quick-start example

Add an XRButton to any object, then extend it:

> [!NOTE]
> XRButtons can operate on objects even if they do not have colliders.

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

### Coding-snippets
Use the EasyXR namespace at the top of your script:
> [!WARNING]
> This namespace is required for XRButton scripts to compile and function correctly.
````csharp
using EasyXRSystems;
````

Handling both hand and body presses:

> [!TIP]
> Using a shared handler method keeps your button logic consistent across hand and body presses and avoids duplicated code.

```csharp
public override void ButtonActivationWithBody(bool isPressed)
    => HandlePress(isPressed, TypesofTouch.Body);

public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
    => HandlePress(isPressed, TypesofTouch.Hand);
 ```

Teleporter example:

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

### Built-in editor tools

- Foldout sections for clean organization  
- Debug Tools that only activate in Play Mode  
- Auto‑disabled UI in Edit Mode  
- Helpful warnings for missing references

### FAQ

**Can my game support EasyXR?**  
Yes. Any Unity project using standard C# workflows can integrate EasyXR without issue.

**Does EasyXR use Update()?**  
No. All interactions are event‑driven for maximum performance.

**Does it work with Gorilla‑Tag‑style physics?**  
Yes. XRButtons were designed with physics‑based VR locomotion in mind.

**How much CPU usage does XRButton use up?**  
XRButton averaged just 0.1% CPU across ~80 buttons in profiler tests, briefly peaking at 0.3% during startup.


## XR Easy Grab
Documentation coming soon!
