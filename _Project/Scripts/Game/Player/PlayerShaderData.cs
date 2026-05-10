using UnityEngine;

namespace TheUnique.Core.Environment
{
    [ExecuteAlways]
    public class PlayerShaderData : MonoBehaviour
    {
        private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPosition");

        private void Update()
        {
            Shader.SetGlobalVector(PlayerPosition, transform.position);
        }
    }
}