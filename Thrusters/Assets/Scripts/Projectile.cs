using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    public string owner = "";
    public int damage = 0;
    public float force = 50f;

    public bool isShockwave;
    public bool isDash;

    [SerializeField] private GameObject _explosionHolder;

    public ParticleSystem explosionParticleSystem;

    void Start()
    {
        if (isShockwave)
        {
            if (isServer)
            {
                RpcExplode(false);
            }
        }

    }

    void Update()
    {
        if (!isShockwave && !isDash)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 1.0f, float.PositiveInfinity), transform.position.z);
        }
        //else
        //{
        //    if (isServer)
        //    {

        //        RpcExplode(false);

        //    }
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other);
        var go = other.gameObject;

        if (go.name == owner || go.name == "Corb" + owner || go.name == "Shockwave" + owner || go.name == "Dash" + owner)
        {
            return;
        }

        Vector3 dir = other.gameObject.transform.position - transform.position;
        dir = dir.normalized;

        if (go.tag == "Player")
        {
            if (isServer)
            {
                var player = go.GetComponent<Player>();

                player.RpcGetPushed(dir, force);
                go.GetComponent<PlayerShoot>().CmdPlayerShot(go.name, damage, owner);
            }
            else
            {
                var rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(dir * force, ForceMode.Impulse);
                    rb.AddForce(dir * force);
                }
            }
        }

        if (go.tag == "Column")
        {
            if (other.isTrigger)
            {
                return;
            }
            if (isServer)
            {
                go.GetComponent<Column>().CmdTakeDamage(damage);
            }
        }

        if (isServer)
        {
            if (!isShockwave)
            {
                RpcExplode(true);
            }
        }

    }

    [ClientRpc]
    void RpcExplode(bool destroy)
    {
        explosionParticleSystem.Play();

        _explosionHolder.transform.SetParent(null);
        if (isServer && destroy)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    var go = collision.gameObject;

    //    if (go.name == owner || go.name == "Corb" + owner)
    //    {
    //        return;
    //    }

    //    Vector3 dir = collision.contacts[0].normal;
    //    dir = -dir.normalized;

    //    if (go.tag == "Player")
    //    {


    //        if (isServer)
    //        {
    //            var player = go.GetComponent<Player>();

    //            player.RpcGetPushed(dir, force);
    //            go.GetComponent<PlayerShoot>().CmdPlayerShot(go.name, damage, owner);
    //        }
    //        else
    //        {
    //            var rb = go.GetComponent<Rigidbody>();
    //            if (rb != null)
    //            {
    //                rb.AddForce(dir * force, ForceMode.Impulse);
    //                rb.AddForce(dir * force);
    //            }

    //        }

    //    }

    //    if (go.tag == "Column")
    //    {
    //        if (isServer)
    //        {
    //            go.GetComponent<Column>().CmdTakeDamage(damage);
    //        }
    //    }

    //    if (isServer)
    //    {
    //        RpcExplode();

    //        explosionParticleSystem.Play();

    //        _explosionHolder.transform.SetParent(null);
    //        NetworkServer.Destroy(gameObject);
    //    }
    //    //if (collision.relativeVelocity.magnitude > 2)
    //    //    Debug.Log("Hard");
    //}

    //[ClientRpc]
    //void RpcExplode()
    //{
    //    if (!isServer)
    //    {
    //        explosionParticleSystem.Play();

    //        _explosionHolder.transform.SetParent(null);
    //    }    
    //}

    //private void OnTriggerEnter(Collider collider)
    //{
    //    RaycastHit hit;
    //    if (Physics.Raycast(transform.position, transform.forward, out hit))
    //    {
    //        Debug.Log("Point of contact: " + hit.point);
    //        Debug.DrawRay(hit.point, hit.normal, Color.green, 1000f);
    //        // Calculate Angle Between the collision point and the player
    //        Vector3 dir = hit.point - collider.transform.position;
    //        // We then get the opposite (-Vector3) and normalize it
    //        dir = dir.normalized;
    //        // And finally we add force in the direction of dir and multiply it by force. 
    //        // This will push back the player

    //        if (collider.tag == "Player")
    //        {
    //            //var rb = collider.GetComponent<Rigidbody>();
    //            //if (rb != null)
    //            //{
    //            //    rb.AddForce(dir * 40, ForceMode.Impulse);
    //            if (isServer)
    //            {
    //                collider.GetComponent<Player>().RpcGetPushed(dir, 40f);
    //                NetworkServer.Destroy(gameObject);
    //            }
    //            //}
    //        }



    //    }



    //}
}
