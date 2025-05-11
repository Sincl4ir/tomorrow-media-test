namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class TurboCharge : BasePowerUp
    {
        public int SpeedMultiplier = 2;

        public override void Apply(Frame f, EntityRef target)
        {
            if (!f.Unsafe.TryGetPointer<Paddle>(target, out var paddle)) { return; }
            if (f.Has<BallSpeedMultiplierBackUp>(target)) { return; }

            f.Add(target, new BallSpeedMultiplierBackUp
            {
                OriginalBallSpeedMult = paddle->BallSpeedMultiplier
            });
            paddle->BallSpeedMultiplier = SpeedMultiplier;

        }

        public override void Remove(Frame f, EntityRef target)
        {
            if (!f.Unsafe.TryGetPointer<Paddle>(target, out var paddle)) { return; };
            if (!f.TryGet<BallSpeedMultiplierBackUp>(target, out var modifier)) { return; }

            paddle->BallSpeedMultiplier = modifier.OriginalBallSpeedMult;
            f.Remove<BallSpeedMultiplierBackUp>(target);
        }
    }
}