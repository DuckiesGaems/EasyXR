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
> [!CAUTION]
> When overriding Unity’s Start method, declare it as `protected override` and call `base.Start();` to ensure XRButton initializes correctly.

```csharp
public class EasyXRTeleporter : XRButton
{
    public Transform destination;
    public GameObject thisgameobject;

        protected override void Start()
        {
            base.Start();
       thisgameobject = this.gameObject;
        }

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

A lightweight, XR‑native climbing and grabbing helper designed for physics‑based VR locomotion.  
Built to be modular, predictable, and easy to integrate into any XR rig.

---

### Features
- Uses **Unity’s XR Input System** (no EasyInputs required)  
- Supports **left and right controllers** via XRNode  
- Detects climbable surfaces using **OverlapSphere**  
- Smooth, physics‑driven climbing motion  
- Optional **non‑rigidbody velocity mode** for smoother movement  
- Fully compatible with **XRDirectInteractor**  
- Event hooks for **OnGrab** and **OnRelease**  
- Works with Gorilla‑Tag‑style locomotion and similar physics systems  

---

### Installation Process

1. **Add the Script to Your Controllers**  
   Attach `XREasyGrab` to both your Left and Right controller objects.

2. **Assign Required References**  
   - **Player** → Your player’s Rigidbody  
   - **Hand Transform** → Usually the controller transform  
   - **Hand Interactor** → Your XRDirectInteractor component  

3. **Configure Input**  
   - Set **XR Hand** to `LeftHand` or `RightHand`  
   - Choose **Interaction Type** (`Trigger` or `Grip`) depending on how you want players to climb

4. **Set Up Climbable Surfaces**  
   - Create a new layer (e.g., `Climbable`)  
   - Assign this layer to any object you want to be climbable  
   - Set the script’s **Climb Layer** to match

5. **Tune Grab Range**  
   Adjust **Grab Range** to control how close the hand must be to grab a surface.

6. **Optional Settings**  
   - **UseNonRBVel** → Enables smoother, non‑rigidbody velocity calculations  
   - **OnGrab / OnRelease** → Add sound effects, particles, haptics, etc.

---

### Example Usage

1. Create a cube  
2. Add a **Collider**  
3. Set its **Layer** to your chosen Climb Layer  
4. Press Play and grab it using your chosen input (trigger or grip)

That’s all you need — climbing is now active.

---

### How It Works (Quick Breakdown)

XR Easy Grab follows a simple, predictable flow:

1. **Detect input**  
   Uses `InputDevice.TryGetFeatureValue()` to read trigger or grip values.

2. **Check for climbable surfaces**  
   Uses `Physics.OverlapSphereNonAlloc()` to find nearby colliders on the Climb Layer.

3. **Create a grab point**  
   A temporary transform is attached to the climbable object at the exact hand position.

4. **Apply climbing velocity**  
   The player’s Rigidbody is moved toward the grab point each physics frame.

5. **Release**  
   When the input is released, the grab point is removed and the collider is re‑enabled.

This keeps the system fast, predictable, and easy to extend.

---

### FAQ

**Does XR Easy Grab require colliders on the hands?**  
No. Only climbable objects need colliders.

**Does it conflict with XRDirectInteractor?**  
No. If the hand is holding an interactable, climbing is automatically disabled.

**Can I add haptics?**  
Yes — use the `OnGrab` and `OnRelease` events.

**Does it work with Gorilla‑Tag‑style movement?**  
Yes. The velocity‑based movement is designed for physics locomotion.


