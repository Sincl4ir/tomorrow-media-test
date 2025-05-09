namespace Tomorrow.Quantum
{
    using global::Quantum;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PowerUpCollisionSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {
        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (!f.Unsafe.TryGetPointer<PowerUpHolder>(info.Entity, out PowerUpHolder* powerUpHolder)) { return; }
            if (!f.Unsafe.TryGetPointer<PlayerLink>(info.Other, out PlayerLink* playerLink)) { return; }

            f.Signals.OnPowerUpCollected(info.Other, powerUpHolder->PowerUp); 
            f.Destroy(info.Entity); 
        }
    }
}
