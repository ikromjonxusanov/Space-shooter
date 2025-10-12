using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3.0f;
    [SerializeField]
    private float _magnetSpeed = 8.0f;

    [SerializeField]
    private int _powerupID;

    [SerializeField]
    private AudioClip _clip;
    
    private bool _isMagnetized = false;
    private Transform _playerTransform;

    void Update()
    {
        if (_isMagnetized && _playerTransform != null)
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            transform.position += direction * _magnetSpeed * Time.deltaTime;
        }
        else
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
        }
        
        if (transform.position.y < -4.5f)
        {
            Destroy(this.gameObject);
        }
    }
    
    public void MagnetizeToPlayer(Transform player)
    {
        _isMagnetized = true;
        _playerTransform = player;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser != null && laser.IsEnemyLaser())
            {
                Destroy(other.gameObject);
                Destroy(this.gameObject);
                return;
            }
        }
        
        if (other.CompareTag("Homing_Missile"))
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
            return;
        }
        
        if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();
            AudioSource.PlayClipAtPoint(_clip, transform.position);
            if (player != null)
            {
                switch (_powerupID)
                {
                    case 0:
                        player.TripleShotActive();
                        break;
                    case 1:
                        player.SpeedBoostActive();
                        break;
                    case 2:
                        player.ShieldActive();
                        break;
                    case 3:
                        player.AmmoRefill();
                        break;
                    case 4:
                        player.Heal();
                        break;
                    case 5:
                        player.SpreadShotActive();
                        break;
                    case 6:
                        player.SlowDownDebuff();
                        break;
                    default:
                        Debug.Log("");
                        break;
                }
            }
            Destroy(this.gameObject);
        }
    }
}
