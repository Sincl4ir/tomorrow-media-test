using Quantum;
using UnityEngine;
public class GrowPowerUpView : QuantumCallbacks
{
    protected Vector3 _originalSize;
    protected bool _atMaxSize = false;
    protected QuantumEntityView _entityRef;

    protected override void OnEnable()
    {
        base.OnEnable();
        QuantumEvent.Subscribe(this, (EventGrowPowerUpApplied e) => HandleGrow(e));
        QuantumEvent.Subscribe(this, (EventGrowPowerUpRemoved e) => ResetGrow(e));

        if (!TryGetComponent<QuantumEntityView>(out _entityRef)) { return; }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        QuantumCallback.UnsubscribeListener(this);
    }

    protected void HandleGrow(EventGrowPowerUpApplied e)
    {
        if (e.Entity != _entityRef.EntityRef) { return; }
        if (_atMaxSize) { return; }

        _originalSize = transform.localScale;
        var scale = transform.localScale;
        scale.x *= e.SizeMultiplier;
        transform.localScale = scale;
        _atMaxSize = true;
    }

    protected void ResetGrow(EventGrowPowerUpRemoved e)
    {
        if (e.Entity != _entityRef.EntityRef) { return; }

        transform.localScale = _originalSize;
        _atMaxSize = false;
    }
}