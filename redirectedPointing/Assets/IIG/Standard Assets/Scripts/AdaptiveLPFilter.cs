/**************************************************************************
 * AdaptiveLPFilter.cs adaptive low pass filter for Unity3D 
 * for positions and orientations (exponential filter)
 * Written by Henrique Galvan Debarba
 * Last update: 03/03/14
 * based on simple low/high pass filters provided by iOS sdk (in objective-C)
 * *************************************************************************/

using UnityEngine;
using System.Collections;


public class AdaptiveLPFilter : MonoBehaviour {
	
    // store current filtered pos/orientation
	private Vector3 m_pos = new Vector3(0,0,0);
	private Quaternion m_orient = new Quaternion(0,0,0,1);

    public float minTranslation = 1.0f;
    public float maxTranslation = 50.0f;

    public float minRotation = 10.0f;
    public float maxRotation = 100.0f;

    public float minFreq = 10.0f;
    public float maxFreq = 100.0f;
	
	public Transform referenceTransf;
	
	void Update(){
		AdaptiveLowPass(referenceTransf.position, Time.deltaTime);
		transform.position = m_pos;
		AdaptiveLowPass(referenceTransf.rotation, Time.deltaTime);
		transform.rotation = m_orient;
	}
	
	float Clamp(float v, float min, float max)
	{
		if(v > max)
			return max;
		else if(v < min)
			return min;
		else
			return v;
	}

	float LowPassSampleRate (float rate, float freq)
	{
		float dt = 1.0f / rate; 
		float RC = 1.0f / freq; 
        return dt / (dt + RC);
	}
    public void AdaptiveLowPass(Vector3 newPos, float deltaTime, float minF, float maxF) {
        minFreq = minF;
        maxFreq = maxF;
        AdaptiveLowPass(newPos, deltaTime);
    }

	public void AdaptiveLowPass(Vector3 newPos, float deltaTime){
		// absolute difference in movement
        float deltaPos = Mathf.Abs(m_pos.magnitude - newPos.magnitude);
        // amount of movement per second
        deltaPos *= (1.0f / deltaTime);
		// clamp to a range of minimal and maximal movement
        deltaPos = Clamp(deltaPos, minTranslation, maxTranslation);
        // normalize (0 to 1)
		deltaPos=(deltaPos-1)/49.0f;
		// diff between min and max filter frequency
        float deltaFreq = maxFreq - minFreq;
		// ADAPT this diff according to movement intensity
        deltaFreq *=deltaPos;
		// offset with the minimum frequency
        deltaFreq += minFreq;

        // compute the fitler value for a given sampling rate and limiting frequency
        float alpha = LowPassSampleRate(1.0f / deltaTime, deltaFreq);
	
        // apply to the position
        m_pos = newPos * alpha + m_pos * (1.0f - alpha);
        //m_pos.x = newPos.x * alpha + m_pos.x * (1.0f - alpha);
        //m_pos.y = newPos.y * alpha + m_pos.y * (1.0f - alpha);
        //m_pos.z = newPos.z * alpha + m_pos.z * (1.0f - alpha);
	}

    public void AdaptiveLowPass(Quaternion newOrient, float deltaTime, float minF, float maxF)
    {
        minFreq = minF;
        maxFreq = maxF;
        AdaptiveLowPass(newOrient, deltaTime);
    } 

	public void AdaptiveLowPass(Quaternion newOrient, float deltaTime){
		// quaternion that represents the difference in rotation
        Quaternion tempQuaternion = newOrient * Quaternion.Inverse(m_orient);
        // convert quaternion to angle axis representation
        float _deltaAngle = 0; 
		Vector3 _axis = new Vector3();
        tempQuaternion.ToAngleAxis(out _deltaAngle, out _axis);
        // absolute difference in angular movemet
        float deltaPos = Mathf.Abs(_deltaAngle);
        // amount of angular movement per second
		deltaPos*=(1.0f/deltaTime);
        // clamp to a range of minimal and maximal ang. movement
        deltaPos = Clamp(deltaPos, minRotation, maxRotation);
        // normalize (0 to 1)
		deltaPos=(deltaPos-1)/49.0f;
        // diff between min and max filter frequency
        float deltaFreq = maxFreq - minFreq;
        // ADAPT this diff according to ang. movement intensity
		deltaFreq *=deltaPos;
        // offset with the minimum frequency
        deltaFreq += minFreq;

        // compute the fitler value for a given sampling rate and limiting frequency
        float alpha = LowPassSampleRate(1.0f / deltaTime, deltaFreq); ;

        // apply to the orientation
		m_orient = Quaternion.Slerp(m_orient,newOrient,alpha);
	}


	public Vector3 GetPos(){
		return m_pos;
	}

	public Quaternion GetOrient(){
		return m_orient;
	}

}
