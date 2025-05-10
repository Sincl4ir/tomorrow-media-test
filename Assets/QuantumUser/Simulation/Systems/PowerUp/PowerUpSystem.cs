namespace Tomorrow.Quantum
{
    using global::Quantum;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PowerUpSystem : SystemMainThreadFilter<PowerUpSystem.Filter>, ISignalOnPowerUpCollected, ISignalOnScoreChanged
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
            f.FreeList(filter.State->ActivePowerUpsList);
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

        public void OnScoreChanged(Frame f, EntityRef ball, EntityRef goal)
        {
            foreach (var pair in f.Unsafe.GetComponentBlockIterator<PlayerLink>())
            {
                var activePowerUps = f.Unsafe.GetPointer<ActivePowerUps>(pair.Entity);
                if (activePowerUps->ActivePowerUpsList == default) { return; }
                var powerUpList = f.ResolveList(activePowerUps->ActivePowerUpsList);
                for (int i = powerUpList.Count - 1; i >= 0; i--)
                {
                    var handler = powerUpList[i];
                    var powerUpAsset = f.FindAsset<BasePowerUp>(handler.PowerUp.Id);

                    if (powerUpAsset != null)
                    {
                        f.Events.OnPowerUpEnded(pair.Entity, handler.PowerUp);
                        powerUpAsset.Remove(f, pair.Entity);
                    }

                    powerUpList.RemoveAt(i);
                }

                f.FreeList(activePowerUps->ActivePowerUpsList);
                activePowerUps->ActivePowerUpsList = default;
                activePowerUps->HasActivePowerUps = false;
            }
        }
    }
}
