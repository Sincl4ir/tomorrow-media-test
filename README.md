# Tomorrow Media - Quantum Test Project

## 📦 Project Summary

This project is an extended version of the provided `tomorrowmedia-test.zip` Quantum project. The following features have been added on top of the initial implementation.

---

## ✅ Features Implemented

### 🧱 Destructible Blocks

- Added several grids of destructible blocks to the scene.
- Blocks are destroyed when hit by the ball **only** if the last to hit the ball was a valid player.
- The AI is not allowed to trigger power-ups.

### 🤖 AI Rival Paddle

- Implemented a basic AI for the second paddle.
- The AI follows the ball along the X-axis using simple and responsive logic.

### ⚡ Power-Up System

A modular and extensible system using `BasePowerUp` assets and an `ActivePowerUps` component.

#### Power-Up Spawn Logic

- 20% chance to spawn a random power-up when a block is destroyed.
- The spawn chance can be configured in the Runtime Config.
- Power-ups fall and must be collected by a player-controlled paddle.
- All active power-ups are removed when the score changes to ensure each round starts fresh.

#### Power-Ups Implemented

1. **Titan Paddle**
   - Doubles the paddle's width for 10 seconds.
   - Automatically reverts to original size.

2. **TurboCharge** *(Creative Power-Up)*
   - Doubles the ball's speed when hit by the paddle.
   - Reverts after 10 seconds.

> Power-ups are applied and removed automatically using a frame-based timer.

### 🖥️ UI & Feedback

- UI displays all active power-ups with icons and remaining duration.
- Visual feedback includes:
  - Icon fill to indicate time remaining.
  - Display of power-up names.

---

## ➕ How to Add a New Power-Up

To add a new power-up:

1. **Create a new class** inheriting from `BasePowerUp`.
2. Override the `Apply(Frame f, EntityRef target)` and `Remove(Frame f, EntityRef target)` methods.
3. Add the new power-up asset to the Runtime Config PowerUp array.
4. The system will automatically handle timing, removal, and UI display.

#### Example

```csharp
public class MyNewPowerUp : BasePowerUp {
    public override void Apply(Frame f, EntityRef target) {
        // Your effect here
    }

    public override void Remove(Frame f, EntityRef target) {
        // Revert effect
    }
}
