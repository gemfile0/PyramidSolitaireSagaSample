using PyramidSolitaireSagaSample.Title.Auth;
using System.Collections;
using Unity.Services.Core;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Title
{
    public class Title : MonoBehaviour
    {
        [SerializeField] private AuthPresenter _authPresenter;

        private IEnumerator Start()
        {
            _authPresenter.SetButtonVisible(false);
            yield return UnityServices.InitializeAsync();

            yield return _authPresenter.SignIn();
        }
    }
}
