var target : Transform;
var distance = 2.0;

var xSpeed = 250.0;
var ySpeed = 120.0;

var zSpeed = 10.0;

var yMinLimit = -20;
var yMaxLimit = 80;

private var x = 0.0;
private var y = 0.0;

@script AddComponentMenu("Camera-Control/Mouse Orbit")

function Start () {
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

	// Make the rigid body not change rotation
   	if (rigidbody)
		rigidbody.freezeRotation = true;
}

function Update () {

    if (target) {
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.R)){
        	x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        	y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
 		
 			y = ClampAngle(y, yMinLimit, yMaxLimit);
 		}
        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
        
        transform.rotation = rotation;
        transform.position = position;
    }
    
    var tgtPos = target.position;
    if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
    {
    	tgtPos -= this.transform.forward * Time.deltaTime * zSpeed;
	    //distance-=0.1;
    }
    if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
    {
    	tgtPos += this.transform.forward * Time.deltaTime * zSpeed;
        //distance+=0.1;
    }
    if (Input.GetMouseButton(2) || Input.GetKey(KeyCode.T)) // pan
    {
    	tgtPos -= this.transform.right * Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime * 0.1f;
    	tgtPos -= this.transform.up * Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime * 0.1f;
    }
    target.position = tgtPos;
}

static function ClampAngle (angle : float, min : float, max : float) {
	if (angle < -360)
		angle += 360;
	if (angle > 360)
		angle -= 360;
	return Mathf.Clamp (angle, min, max);
}

