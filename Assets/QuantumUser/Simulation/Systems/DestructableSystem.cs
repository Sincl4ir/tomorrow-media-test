namespace Tomorrow.Quantum
{
    using global::Quantum;
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class DestructableSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {
        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (!f.Unsafe.TryGetPointer<Destructable>(info.Entity, out Destructable* destructable)) { return; }
            if (!f.Unsafe.TryGetPointer<Ball>(info.Other, out Ball* ball)) { return; }
            if (!f.Unsafe.TryGetPointer<PlayerLink>(ball->Paddle, out PlayerLink* player)) { return; }

            if (f.RNG->Next(0, 100) > f.RuntimeConfig.PowerUpChance) { return; }

            // Get random powerUp
            var powerUps = f.RuntimeConfig.PowerUps;
            if (powerUps == null || powerUps.Length == 0) { return; }

            int index = f.RNG->Next(0, powerUps.Length);
            var powerUpRef = powerUps[index];
            var powerUp = f.FindAsset(powerUpRef);

            // Spawn power-up 
            var entity = f.Create(powerUp.Prototype);

            var handler = new PowerUpHolder
            {
                PowerUp = powerUp
            };

            f.Add(entity, handler);

            if (!f.Unsafe.TryGetPointer<Transform3D>(entity, out Transform3D* powerUpTr)) { return; }
            if (!f.Unsafe.TryGetPointer<Transform3D>(info.Entity, out Transform3D* destructableTr)) { return; }
            if (!f.Unsafe.TryGetPointer<Transform3D>(ball->Paddle, out Transform3D* paddleTr)) { return; }

            // Align X to paddle and keep Y/Z from destructable
            powerUpTr->Position = new FPVector3(destructableTr->Position.X, paddleTr->Position.Y, paddleTr->Position.Z);

            // Destroy desctructable 
            f.Destroy(info.Entity);
        }
    }
}
