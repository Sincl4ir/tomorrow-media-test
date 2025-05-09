using Quantum;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class AssetRefComparer : IEqualityComparer<AssetRef<BasePowerUp>>
{
    public bool Equals(AssetRef<BasePowerUp> x, AssetRef<BasePowerUp> y) => x.Id == y.Id;
    public int GetHashCode(AssetRef<BasePowerUp> obj) => obj.Id.GetHashCode();

}
public class PowerUpUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _powerUpText;

    private readonly Dictionary<AssetRef<BasePowerUp>, string> _activePowerUps = new(new AssetRefComparer());
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
        if (!frame.TryGet<PlayerLink>(e.Entity, out var link)) return;
        if (!QuantumRunner.Default.Game.PlayerIsLocal(link.Player)) return;

        _player = e.Entity;
        var name = frame.FindAsset<BasePowerUp>(e.PowerUpRef.Id).PowerUpName;
        _activePowerUps[e.PowerUpRef] = name;
    }

    private void OnPowerUpEnded(EventOnPowerUpEnded e)
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (!frame.TryGet<PlayerLink>(e.Entity, out var link)) return;
        if (!QuantumRunner.Default.Game.PlayerIsLocal(link.Player)) return;

        //MARK FOR REMOVAL IT WILL THROW ERROR
        _activePowerUps.Remove(e.PowerUpRef);
    }

    void Update()
    {
        if (!_activated || _activePowerUps.Count == 0)
        {
            _powerUpText.text = "";
            return;
        }

        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (!frame.TryGet<ActivePowerUps>(_player, out var activePowerUps)) { return; }

        var list = frame.ResolveList(activePowerUps.ActivePowerUpsList);

        if (list.Count == 0)
        {
            _powerUpText.text = "";
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (var powerUpHandler in list)
        {
            if (_activePowerUps.TryGetValue(powerUpHandler.PowerUp, out var name))
            {
                var secondsLeft = powerUpHandler.Timer.TimeLeft.AsFloat;
                sb.AppendLine($"{name}: {secondsLeft:F1}s");
            }
        }

        _powerUpText.text = sb.ToString();
    }

    //    void Update()
    //    {

    //        if (!_activated) { return; }
    //        if (_activePowerUps.Count == 0)
    //        {
    //            _powerUpText.text = "";
    //            _powerUpIcon.gameObject.SetActive(false); // Hide icon if no power-ups are active
    //            _powerUpIcon.fillAmount = 0f; // Reset fill to 0
    //            return;
    //        }

    //        var frame = QuantumRunner.Default.Game.Frames.Verified;
    //        _powerUpText.text = "";

    //        if (!frame.TryGet<PowerUpHandler>(Player, out var handler)) { return; }

    //        // Find the corresponding power-up for the handler
    //        foreach (var kvp in _activePowerUps)
    //        {
    //            if (handler.PowerUp.Id == kvp.Key.Id)
    //            {
    //                // Display the power-up name
    //                _powerUpText.text = $"{kvp.Value}: ";

    //                var icon = frame.FindAsset<BasePowerUp>(handler.PowerUp.Id).Icon;
    //                // Set the power-up icon (if it has one)
    //                _powerUpIcon.sprite = icon;
    //                _powerUpIcon.gameObject.SetActive(true); // Ensure the icon is visible

    //                // Calculate the normalized progress and remaining time
    //                float normalized = handler.Timer.NormalizedProgress.AsFloat;
    //                float secondsLeft = handler.Timer.TimeLeft.AsFloat;

    //                // Update the fill amount based on the normalized progress (0 to 1)
    //                _powerUpIcon.fillAmount = normalized;

    //                // Append time remaining in seconds
    //                //_powerUpText.text += $"{secondsLeft:F1}s ({normalized:P0})";
    //                break;
    //            }
    //        }
    //    }
}
