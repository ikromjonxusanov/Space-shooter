using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [SerializeField]
    private float _speed = 4f;

    [SerializeField]
    private float _rotationSpeed = 150f;

    [SerializeField]
    private float _lifetime = 8f;

    private Transform _target;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
        }

        Destroy(gameObject, _lifetime);
    }

    void FixedUpdate()
    {
        if (_target == null)
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
            return;
        }

        Vector2 direction = (Vector2)_target.position - _rb.position;
        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        _rb.angularVelocity = -rotateAmount * _rotationSpeed;

        _rb.velocity = transform.up * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser != null && !laser.IsEnemyLaser())
            {
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject, 0.5f);
    }
}
