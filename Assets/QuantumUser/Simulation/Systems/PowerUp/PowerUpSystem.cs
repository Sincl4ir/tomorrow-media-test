namespace Tomorrow.Quantum
{
    using global::Quantum;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PowerUpSystem : SystemMainThreadFilter<PowerUpSystem.Filter>, ISignalOnPowerUpCollected
    {
        public unsafe struct Filter
        {
            public EntityRef Entity;
            public PowerUpHandler* State;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var def = filter.State;
            if (!def->IsActive) { return; }

            def->Timer.Tick(f);
            if (!def->Timer.IsDone) { return; }

            var powerUp = f.FindAsset(def->PowerUp);
            if (powerUp == null) { return; }

            f.Events.OnPowerUpEnded(filter.Entity, filter.State->PowerUp);
            powerUp.Remove(f, filter.Entity);
            def->IsActive = false;
        }

        public void OnPowerUpCollected(Frame f, EntityRef entity, AssetRef<BasePowerUp> powerUpRef)
        {
            var powerUp = f.FindAsset<BasePowerUp>(powerUpRef.Id);
            if (powerUp == null)
            {
                Log.Error($"[PowerUpSystem] Could not find asset {powerUpRef}");
                return;
            }

            var def = new PowerUpHandler
            {
                PowerUp = powerUpRef,
                Timer = new Timer(), 
                IsActive = true
            };

            def.Timer.Start(powerUp.Duration);
            f.Add(entity, def);

            f.Events.OnPowerUpStarted(entity, powerUpRef);
            powerUp.Apply(f, entity);
        }
    }
}
