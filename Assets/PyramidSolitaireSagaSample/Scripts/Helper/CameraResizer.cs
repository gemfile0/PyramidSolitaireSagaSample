using PyramidSolitaireSagaSample.System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public enum CameraResizerScreenMathMode
    {
        Width,
        Height,
        Fit
    }

    public interface ICameraResizer
    {
        (Vector3 cameraCenterPosition, Vector2 cameraHalfSize) GetCameraHalfSize();
    }

    public interface ICameraResizerSettter
    {
        ICameraResizer CameraResizer { set; }
    }

    [RequireComponent(typeof(Camera))]
    public class CameraResizer : MonoBehaviour, IGameObjectFinderSetter, ICameraResizer
    {
        [SerializeField] private Vector2 _referenceResolution;
        [SerializeField] private CameraResizerScreenMathMode _screenMatchMode;

        private Camera _camera;
        private Transform _cachedTransform;
        private float _latestTargetRatio;
        private float _latestScreenRatio;

        private Vector2 _cameraHalfSize;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            foreach (var cameraResizerSetter in finder.FindGameObjectOfType<ICameraResizerSettter>())
            {
                cameraResizerSetter.CameraResizer = this;
            }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _cachedTransform = transform;
            UpdateCameraSize();
        }

        private void Update()
        {
            UpdateCameraSize();
        }

        private void UpdateCameraSize()
        {
            float targetRatio = _referenceResolution.x / _referenceResolution.y;
            float screenRatio = Screen.width / (float)Screen.height;

            //Debug.Log($"UpdateCameraSize : {screenRatio} vs {targetRatio}");
            if (_latestTargetRatio != targetRatio || _latestScreenRatio != screenRatio)
            {
                _latestTargetRatio = targetRatio;
                _latestScreenRatio = screenRatio;

                if (_screenMatchMode == CameraResizerScreenMathMode.Width)
                {
                    float ratioFactor = targetRatio / screenRatio;
                    if (screenRatio >= targetRatio)
                    {
                        _camera.orthographicSize = _referenceResolution.y / 200f;
                    }
                    else
                    {
                        _camera.orthographicSize = (_referenceResolution.y / 200f) * ratioFactor;
                    }
                }
                else if (_screenMatchMode == CameraResizerScreenMathMode.Height)
                {
                    _camera.orthographicSize = _referenceResolution.y / 200f;
                }
                else if (_screenMatchMode == CameraResizerScreenMathMode.Fit)
                {
                    float ratioFactor = targetRatio / screenRatio;
                    _camera.orthographicSize = (_referenceResolution.y / 200f) * ratioFactor;
                }

                float halfHeight = _camera.orthographicSize;
                float halfWidth = _camera.aspect * halfHeight;
                _cameraHalfSize = new Vector2(halfWidth, halfHeight);
            }
        }

        public (Vector3 cameraCenterPosition, Vector2 cameraHalfSize) GetCameraHalfSize()
        {
            return (_cachedTransform.position, _cameraHalfSize);
        }
    }
}
