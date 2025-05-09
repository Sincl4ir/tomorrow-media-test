namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class GrowPowerUp : BasePowerUp
    {
        public int SizeMultiplier = 2;

        public override void Apply(Frame f, EntityRef target)
        {
            if (!f.Unsafe.TryGetPointer<PhysicsCollider3D>(target, out var collider)) { return; }
            if (!f.Unsafe.TryGetPointer<Transform3D>(target, out var transform)) { return; }
            if (f.Has<OriginalPaddleSize>(target)) { return; }

            // Store current extents
            var extents = collider->Shape.Box.Extents;

            f.Add(target, new OriginalPaddleSize
            {
                originalX = extents.X,
                originalY = extents.Y,
                originalZ = extents.Z
            });

            Log.Debug($"X value is {extents.X}, new value is {extents.X * SizeMultiplier}");
            // Apply multiplier only to X
            collider->Shape.Box.Extents = new FPVector3(
                extents.X * SizeMultiplier,
                extents.Y,
                extents.Z
            );

            //collider->Shape.Box.Extents *= SizeMultiplier;
            transform->Position.Y = 0;

            // Calculate the ratio and send it
            f.Events.GrowPowerUpApplied(target, SizeMultiplier);
        }

        public override void Remove(Frame f, EntityRef target)
        {
            if (!f.Unsafe.TryGetPointer<PhysicsCollider3D>(target, out var collider)) { return; }
            if (!f.Unsafe.TryGetPointer<Transform3D>(target, out var transform)) { return; }
            if (!f.TryGet<OriginalPaddleSize>(target, out var backup)) { return; }

            collider->Shape.Box.Extents = new FPVector3(backup.originalX, backup.originalY, backup.originalZ);
            transform->Position.Y = 0;
            f.Remove<OriginalPaddleSize>(target);

            f.Events.GrowPowerUpRemoved(target);
        }
    }
}