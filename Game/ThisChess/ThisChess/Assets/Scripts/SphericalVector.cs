using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

[Serializable]
public struct SphericalVector {
    public float length;
    public float zenith;
    public float azimuth;

    public SphericalVector(float l, float z, float a) {
        length = l;
        zenith = z;
        azimuth = a;
    }
    public Vector3 Position {
        get {
            return length * Direction;
        }
    }
    public Vector3 Direction {
        get {
            Vector3 direction = Vector3.zero;
            direction.y = Mathf.Sin(zenith * Mathf.PI / 2);
            direction.x = Mathf.Cos(zenith * Mathf.PI / 2) * Mathf.Sin(azimuth * Mathf.PI);
            direction.z = Mathf.Cos(zenith * Mathf.PI / 2) * Mathf.Cos(azimuth * Mathf.PI);
            return direction;
        }
    }

}
