namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe abstract class BasePowerUp : AssetObject
    {
#if QUANTUM_UNITY
        public Sprite Icon;
#endif
        public string PowerUpName;
        public FP Duration;
        public AssetRef<EntityPrototype> Prototype;

        public abstract void Apply(Frame f, EntityRef target);
        public abstract void Remove(Frame f, EntityRef target);
    }
}
