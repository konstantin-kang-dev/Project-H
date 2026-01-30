using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LaggingLamp : MonoBehaviour
{
    [SerializeField] Light _light;
    [SerializeField] bool _enableToggle = false;
    [SerializeField] float _minToggleInterval = 0.5f;
    [SerializeField] float _maxToggleInterval = 2f;

    [SerializeField] float _minLagInterval = 0.1f;
    [SerializeField] float _maxLagInterval = 0.1f;

    [SerializeField] float _minLagIntensity = 1f;
    [SerializeField] float _maxLagIntensity  = 1f;

    float _toggleTimer = 0;
    float _nextToggleInterval = 0;

    bool _lightState = true;

    float _lagTimer = 0;
    bool _lightLagState = false;
    float _nextLagInterval = 0;

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

            if (_lagTimer >= _nextLagInterval)
            {
                _lagTimer = 0;
                _nextLagInterval = Random.Range(_minLagInterval, _maxLagInterval);

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
            float intensity = Random.Range(_minLagIntensity, _maxLagIntensity);
            SetLightIntensity(intensity);
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
