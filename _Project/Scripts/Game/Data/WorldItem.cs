using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TheUnique.Core.Items;
using TheUnique.Core.Player;

namespace TheUnique.Core.World
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class WorldItem : MonoBehaviour
    {
        [SerializeField] private ItemSO _itemData;
        [SerializeField] private int _amount = 1;
        
        [Header("Настройки магнита")]
        [SerializeField] private float _pickUpDistance = 2.5f;
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _magnetDelay = 0.5f;
        
        private bool _isGathered = false;
        private bool _canMagnetize = false;
        private Rigidbody2D _rb;

        public void Initialize(ItemSO data, int amount)
        {
            _itemData = data;
            _amount = amount;
            
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = data.icon;

            _rb = GetComponent<Rigidbody2D>();

            CircleCollider2D magnetZone = gameObject.AddComponent<CircleCollider2D>();
            magnetZone.isTrigger = true;
            magnetZone.radius = _pickUpDistance;

            if (_rb != null)
            {
                Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
                _rb.AddForce(randomDir * 4f, ForceMode2D.Impulse);
            }

            StartCoroutine(EnableMagnetDelay());
        }

        private IEnumerator EnableMagnetDelay()
        {
            yield return new WaitForSeconds(_magnetDelay);
            _canMagnetize = true;
        }
        
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (_isGathered || !_canMagnetize) return;

            if (collision.CompareTag("Player"))
            {
                StartCoroutine(FlyToPlayer(collision.transform));
            }
        }

        private IEnumerator FlyToPlayer(Transform player)
        {
            _isGathered = true;
            
            yield return new WaitForFixedUpdate(); 

            if (_rb) _rb.simulated = false;

            while (Vector2.Distance(transform.position, player.position) > 0.2f)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            var playerEntity = player.GetComponent<PlayerEntity>();
            if (playerEntity != null && playerEntity.Inventory.AddItem(_itemData, _amount))
            {
                Destroy(gameObject);
            }
            else
            {
                _isGathered = false;
                if (_rb) _rb.simulated = true;
            }
        }
    }
}