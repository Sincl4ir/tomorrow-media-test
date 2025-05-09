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
            public ActivePowerUps* State;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (!filter.State->HasActivePowerUps) { return; }

            var activePowerUps = f.ResolveList(filter.State->ActivePowerUpsList);
            for (int i = activePowerUps.Count - 1; i >= 0; --i)
            {
                var powerUpData = activePowerUps[i];
                powerUpData.Timer.Tick(f); 

                if (!powerUpData.Timer.IsDone)
                {
                    activePowerUps[i] = powerUpData;
                    continue;
                }

                var powerUpAsset = f.FindAsset<BasePowerUp>(powerUpData.PowerUp.Id);
                if (powerUpAsset != null)
                {
                    f.Events.OnPowerUpEnded(filter.Entity, powerUpData.PowerUp);
                    powerUpAsset.Remove(f, filter.Entity);
                }

                // Remove expired power-up
                activePowerUps.RemoveAt(i);
            }

            if (activePowerUps.Count > 0) { return; }
            filter.State->HasActivePowerUps = false;

            // Copy-pasted from Quantum Docs :)

            // A component HAS TO de-allocate all collection it owns from the frame data, otherwise it will lead to a memory leak.
            // receives the list QListPtr reference.
            f.FreeList(filter.State->ActivePowerUpsList);

            // All dynamic collections a component points to HAVE TO be nullified in a component's OnRemoved
            // EVEN IF is only referencing an external one!
            // This is to prevent serialization issues that otherwise lead to a desynchronisation.
            filter.State->ActivePowerUpsList = default;
        }

        public void OnPowerUpCollected(Frame f, EntityRef entity, AssetRef<BasePowerUp> powerUpRef)
        {
            var powerUpAsset = f.FindAsset<BasePowerUp>(powerUpRef.Id);
            if (powerUpAsset == null)
            {
                Log.Error($"[PowerUpSystem] Could not find asset {powerUpRef}");
                return;
            }

            if (!f.Has<ActivePowerUps>(entity))
            {
                f.Add(entity, new ActivePowerUps { });
            }

            var activePowerUps = f.Unsafe.GetPointer<ActivePowerUps>(entity);
            if (activePowerUps->ActivePowerUpsList == default)
            {
                activePowerUps->ActivePowerUpsList = f.AllocateList<PowerUpHandler>();
            }

            activePowerUps->HasActivePowerUps = true;
            var powerUpList = f.ResolveList(activePowerUps->ActivePowerUpsList);

            foreach (var handler in powerUpList)
            {
                if (handler.PowerUp == powerUpRef)
                {
                    Log.Debug("PowerUp already exists, no need to add again");
                    return;
                }

            }
            var powerUpData = new PowerUpHandler
            {
                PowerUp = powerUpRef,
                Timer = new Timer()
            };
            powerUpData.Timer.Start(powerUpAsset.Duration);
            powerUpList.Add(powerUpData);

            f.Events.OnPowerUpStarted(entity, powerUpRef);
            powerUpAsset.Apply(f, entity);
        }
    }
}
