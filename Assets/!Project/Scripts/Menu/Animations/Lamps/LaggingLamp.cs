using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LaggingLamp : MonoBehaviour
{
    [SerializeField] Light _light;
    [SerializeField] bool _enableToggle = false;
    [SerializeField] float _minToggleInterval = 0.5f;
    [SerializeField] float _maxToggleInterval = 2f;
    [SerializeField] float _lagInterval = 0.1f;

    float _toggleTimer = 0;
    float _nextToggleInterval = 0;

    bool _lightState = true;

    float _lagTimer = 0;
    bool _lightLagState = false;

    float _initialIntensity = 0;
    void Awake()
    {
        _initialIntensity = _light.intensity;
    }

    private void FixedUpdate()
    {
        if (_enableToggle)
        {
            _toggleTimer += Time.fixedDeltaTime;

            if (_toggleTimer >= _nextToggleInterval)
            {
                _toggleTimer = 0;
                _nextToggleInterval = Random.Range(_minToggleInterval, _maxToggleInterval);
                SetLightState(!_lightState);
            }
        }

        if (_lightState || !_enableToggle)
        {
            _lagTimer += Time.fixedDeltaTime;

            if (_lagTimer >= _lagInterval)
            {
                _lagTimer = 0;
                ToggleLag();
            }
        }

    }

    void SetLightState(bool value)
    {
        _lightState = value;
        _light.intensity = value ? _initialIntensity : 0f;
    }

    void ToggleLag()
    {
        _lightLagState = !_lightLagState;

        if (_lightLagState)
        {
            SetLightIntensity(0.85f);
        }
        else
        {
            SetLightIntensity(1f);
        }
    }
    void SetLightIntensity(float multiplier)
    {
        _light.intensity = _initialIntensity * multiplier;
    }

    void Update()
    {
        
    }
}
