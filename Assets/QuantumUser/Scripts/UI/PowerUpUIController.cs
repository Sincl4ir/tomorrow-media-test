using Quantum;
using System.Collections.Generic;
using UnityEngine;

public class AssetRefComparer : IEqualityComparer<AssetRef<BasePowerUp>>
{
    public bool Equals(AssetRef<BasePowerUp> x, AssetRef<BasePowerUp> y) => x.Id == y.Id;
    public int GetHashCode(AssetRef<BasePowerUp> obj) => obj.Id.GetHashCode();
}

public class PowerUpUIController : MonoBehaviour
{
    [SerializeField] private Transform _uiContainer; 
    [SerializeField] private PowerUpUIEntry _powerUpUIPrefab;

    private readonly Dictionary<AssetRef<BasePowerUp>, PowerUpUIEntry> _activeUIEntries = new(new AssetRefComparer());
    private EntityRef _player;
    private bool _activated;

    void OnEnable()
    {
        QuantumEvent.Subscribe(this, (EventOnPowerUpStarted e) => OnPowerUpStarted(e));
        QuantumEvent.Subscribe(this, (EventOnPowerUpEnded e) => OnPowerUpEnded(e));
        QuantumEvent.Subscribe(this, (EventOnGameStateChanged e) => OnGameStateChanged(e));
    }

    void OnDisable()
    {
        QuantumCallback.UnsubscribeListener(this);
    }

    private void OnGameStateChanged(EventOnGameStateChanged e)
    {
        _activated = e.state == GameState.Playing;
    }

    private void OnPowerUpStarted(EventOnPowerUpStarted e)
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (!frame.TryGet<PlayerLink>(e.Entity, out var link)) { return; }
        if (!QuantumRunner.Default.Game.PlayerIsLocal(link.Player)) { return; }

        _player = e.Entity;
        var powerUp = frame.FindAsset<BasePowerUp>(e.PowerUpRef.Id);
        var name = powerUp.PowerUpName;

        if (_activeUIEntries.ContainsKey(e.PowerUpRef)) { return; }

        var instance = Instantiate(_powerUpUIPrefab, _uiContainer);
        instance.NameText.text = name;
        instance.FillImage.sprite = powerUp.Icon;
        instance.FillImage.fillAmount = 1.0f;

        _activeUIEntries[e.PowerUpRef] = instance;
    }

    private void OnPowerUpEnded(EventOnPowerUpEnded e)
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (!frame.TryGet<PlayerLink>(e.Entity, out var link)) { return; }
        if (!QuantumRunner.Default.Game.PlayerIsLocal(link.Player)) { return; }

        if (_activeUIEntries.TryGetValue(e.PowerUpRef, out var ui))
        {
            Destroy(ui.gameObject);
            _activeUIEntries.Remove(e.PowerUpRef);
        }
    }

    void Update()
    {
        if (!_activated || _activeUIEntries.Count == 0) { return; }

        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (!frame.TryGet<ActivePowerUps>(_player, out var activePowerUps)) { return; }

        var list = frame.ResolveList(activePowerUps.ActivePowerUpsList);

        foreach (var powerUpHandler in list)
        {
            if (_activeUIEntries.TryGetValue(powerUpHandler.PowerUp, out var ui))
            {
                var progress = Mathf.Clamp01(powerUpHandler.Timer.TimeLeft.AsFloat / powerUpHandler.Timer.TotalTime.AsFloat);
                ui.FillImage.fillAmount = progress;
            }
        }
    }
}
