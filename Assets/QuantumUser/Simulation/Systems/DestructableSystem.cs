namespace Tomorrow.Quantum
{
    using global::Quantum;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class DestructableSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {
        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (!f.Unsafe.TryGetPointer<Destructable>(info.Entity, out Destructable* destructable)) { return; }
            if (!f.Unsafe.TryGetPointer<Ball>(info.Other, out Ball* ball)) { return; }
            if (f.RNG->Next(0, 100) > f.RuntimeConfig.PowerUpChance) { return; }
            f.Destroy(info.Entity);
        }
    }
}
