using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// An enum to track the possible states of FloatingScore
public enum FSState
{
    idle,
    pre,
    active,
    post
}

public class FloatingScore : MonoBehaviour {

    public FSState state = FSState.idle;
    [SerializeField]
    private int _score = 0; // The score field
    public string scoreString;
    public Text textRef;

    void Start()
    {
        textRef = GetComponent<Text>();
    }
    
    // The score property also sets scoreString when set
    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
            textRef = GetComponent<Text>();
            textRef.text = scoreString;
        }
    }

    public List<Vector3> bezierPts; // Bezier points for movement
    public List<float> fontSizes;   // Bezier points for font scaling
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut; // Uses Easing in Utils.cs

    // The GameObject that will recieve the SendMessage when this is done moving
    public GameObject reportFinishTo = null;

    // Set up the FloatingScore and movement
    // Note the use of parameter deaults for eTimeS & eTimeD
    public void Init(List<Vector3> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        bezierPts = new List<Vector3>(ePts);
        if (ePts.Count == 1) // If there's only one option
        {
            // ... then just go there
            transform.position = ePts[0];
            return;
        }

        // If eTimeS is the default, just start at the current time
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = FSState.pre; // Set it to the pre state, ready to start moving
    }

    public void FSCallback(FloatingScore fs)
    {
        // When this callback is called by SendMessage, add the score from the calling FloatingScore
        score += fs.score;
    }
	
	// Update is called once per frame
	void Update () {
        // If this is not moving, just return
        if (state == FSState.idle) return;

        // Get u from the curret time and duration
        // u ranges from 0 to 1 (usually)
        float u = (Time.time - timeStart) / timeDuration;
        // Use Easing class from Utils to curve the u value
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0) // If u < 0, then we shouldn't move yet
        {
            state = FSState.pre;
            // Move to the initial point
            transform.position = bezierPts[0];
        }
        else
        {
            if (u >= 1) // If u >= 1, we're done moving
            {
                uC = 1; // Set uC = 1 so we don't overshoot
                state = FSState.post;
                if (reportFinishTo != null) // If there's a callback GameObject
                {
                    // Use SendMessage to call the FSCallback method with this as the parameter
                    reportFinishTo.SendMessage("FSCallback", this);
                    // Now that the message has been sent, destroy this gameObject
                    Destroy(gameObject);
                }
                else // If there's nothing to callback
                {
                    // ... Then don't destroy this. Just let it stay still.
                    state = FSState.idle;
                }
            }
            else
            {
                // 0 <= u < 1, which means that this is active and moving
                state = FSState.active;
            }
            // Use Bezier curve to move this to the right point
            Vector3 pos = Utils.Bezier(uC, bezierPts);
            transform.position = pos;
            if(fontSizes != null && fontSizes.Count > 0)
            {
                // If fontSizes has values in it, then adjust the fontSizes of this UI Text
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                textRef.fontSize = size;
            }
        }
	}
}
