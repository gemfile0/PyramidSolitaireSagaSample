using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Title.Auth
{
    public class AuthPresenter : MonoBehaviour
    {
        [SerializeField] private AuthModel _authModel;
        [SerializeField] private AuthUI _authUI;

        private void OnEnable()
        {
            _authUI.onPlayClick += OnPlayClick;
            _authUI.onSaveProgressClick += OnSaveProgressClick;
        }

        private void OnDisable()
        {
            _authUI.onPlayClick -= OnPlayClick;
            _authUI.onSaveProgressClick -= OnSaveProgressClick;
        }

        internal void SetButtonVisible(bool value)
        {
            _authUI.SetButtonVisible(value);
        }

        private async void OnPlayClick()
        {
            _authUI.SetPlayButtonInteractable(false);
            await SignInAnonymouslyAsync();
        }

        private void OnSaveProgressClick()
        {
            
        }

        internal IEnumerator SignIn()
        {
            if (AuthenticationService.Instance.SessionTokenExists == false)
            {
                SetButtonVisible(true);
            }
            else
            {
                yield return SignInAnonymouslyCoroutine();
            }
        }

        private IEnumerator SignInAnonymouslyCoroutine()
        {
            Task signInTask = SignInAnonymouslyAsync();
            while (signInTask.IsCompleted == false)
            {
                yield return null;
            }
        }

        async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");

                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }
    }
}
